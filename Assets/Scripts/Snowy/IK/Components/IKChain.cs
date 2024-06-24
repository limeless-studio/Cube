using System.Collections.Generic;
using Snowy.IK;
using Snowy.Utils;
using UnityEngine;

namespace Snowy.ActiveRagdoll.IK.Components
{
    public class IKChain : IKComponent
    {
        [SerializeField] Transform target;
        [SerializeField] Transform pole;
        [SerializeField] Transform startBone;
        [SerializeField] int chainLength;
        
        private Transform[] bones;
        
        [SerializeField] int iterations = 10;
        [SerializeField] float delta = 0.001f;
        
        [SerializeField, Range(0, 1)] float snapBackStrength = 0.99f;
        
        [SerializeField] protected IKBone[] BonesData;
        protected float CompleteLength;
        protected Quaternion StartRotationTarget;
        protected Transform Root;
        

        public override void Init(IKSolver ikSolver)
        {
            base.Init(ikSolver);
            
            // Init
            Root = startBone;
            bones = new Transform[chainLength + 1];
            
            for (var i = 0; i <= chainLength; i++)
            {
                if (Root == null)
                    throw new UnityException("The chain value is longer than the ancestor chain!");
                Root = Root.parent;
            }
            
            BonesData = new IKBone[chainLength + 1];
            
            if (!target)
            {
                #if UNITY_EDITOR
                Debug.LogWarning($"Target not set for IKChain in {Solver?.name}");
                #endif
                return;
            }
            
            StartRotationTarget = Utilities.GetRotationWithRespect(target, Root);

            var currentBone = startBone;
            CompleteLength = 0;
            for (var i = bones.Length - 1; i >= 0; i--)
            {
                bones[i] = currentBone;
                var bone = new IKBone
                {
                    Bone = currentBone,
                    StartRotation = Utilities.GetRotationWithRespect(currentBone, Root), 
                    Position = Utilities.GetPositionWithRespect(currentBone, Root)
                };

                if (i == bones.Length - 1)
                {
                    bone.StartDirection = Utilities.GetPositionWithRespect(target, Root) - Utilities.GetPositionWithRespect(currentBone, Root);;
                } else
                {
                    bone.StartDirection = Utilities.GetPositionWithRespect(bones[i + 1], Root) -
                                          Utilities.GetPositionWithRespect(currentBone, Root);
                    bone.Length = bone.StartDirection.magnitude;
                    CompleteLength += bone.Length;
                }
                
                BonesData[i] = bone;
                currentBone = currentBone.parent;
            }
        }

        public override void Solve()
        {
            if (target == null || Solver == null)
                return;

            if (bones.Length != chainLength + 1)
                Init(Solver);

            //Fabric

            //  root
            //  (bone0) (bonelen 0) (bone1) (bonelen 1) (bone2)...
            //   x--------------------x--------------------x---...

            //get position
            for (int i = 0; i < BonesData.Length; i++)
                BonesData[i].Position = Utilities.GetPositionWithRespect(BonesData[i].Bone, Root);

            var targetPosition = Utilities.GetPositionWithRespect(target, Root);
            var targetRotation = Utilities.GetRotationWithRespect(target, Root);

            //1st is possible to reach?
            if ((targetPosition - Utilities.GetPositionWithRespect(bones[0], Root)).sqrMagnitude >= CompleteLength * CompleteLength)
            {
                //just strech it
                var direction = (targetPosition - BonesData[0].Position).normalized;
                //set everything after root
                for (int i = 1; i < BonesData.Length; i++)
                    BonesData[i].Position = BonesData[i - 1].Position + direction * BonesData[i - 1].Length;
            }
            else
            {
                for (int i = 0; i < BonesData.Length - 1; i++)
                    BonesData[i + 1].Position = Vector3.Lerp(BonesData[i + 1].Position, BonesData[i].Position + BonesData[i].StartDirection, snapBackStrength);

                for (int iteration = 0; iteration < iterations; iteration++)
                {
                    //https://www.youtube.com/watch?v=UNoX65PRehA
                    //back
                    for (int i = BonesData.Length - 1; i > 0; i--)
                    {
                        if (i == BonesData.Length - 1)
                            BonesData[i].Position = targetPosition; //set it to target
                        else
                            BonesData[i].Position = BonesData[i + 1].Position + (BonesData[i].Position - BonesData[i + 1].Position).normalized * BonesData[i].Length; //set in line on distance
                    }

                    //forward
                    for (int i = 1; i < BonesData.Length; i++)
                        BonesData[i].Position = BonesData[i - 1].Position + (BonesData[i].Position - BonesData[i - 1].Position).normalized * BonesData[i - 1].Length;

                    //close enough?
                    if ((BonesData[BonesData.Length - 1].Position - targetPosition).sqrMagnitude < delta * delta)
                        break;
                }
            }

            //move towards pole
            if (pole != null)
            {
                var polePosition = Utilities.GetPositionWithRespect(pole, Root);
                for (int i = 1; i < BonesData.Length - 1; i++)
                {
                    var plane = new Plane(BonesData[i + 1].Position - BonesData[i - 1].Position, BonesData[i - 1].Position);
                    var projectedPole = plane.ClosestPointOnPlane(polePosition);
                    var projectedBone = plane.ClosestPointOnPlane(BonesData[i].Position);
                    var angle = Vector3.SignedAngle(projectedBone - BonesData[i - 1].Position, projectedPole - BonesData[i - 1].Position, plane.normal);
                    BonesData[i].Position = Quaternion.AngleAxis(angle, plane.normal) * (BonesData[i].Position - BonesData[i - 1].Position) + BonesData[i - 1].Position;
                }
            }

            //set position & rotation
            for (int i = 0; i < BonesData.Length; i++)
            {
                //if (!useJoint)
                //{
                    if (i == BonesData.Length - 1)
                        Utilities.SetRotationWithRespect(bones[i], Quaternion.Inverse(targetRotation) * StartRotationTarget * Quaternion.Inverse(BonesData[i].StartRotation), Root);
                    else
                        Utilities.SetRotationWithRespect(bones[i], Quaternion.FromToRotation(BonesData[i].StartDirection, BonesData[i + 1].Position - BonesData[i].Position) * Quaternion.Inverse(BonesData[i].StartRotation), Root);
                    Utilities.SetPositionWithRespect(bones[i], BonesData[i].Position, Root);
                    //if (!onlyRotate) SetPositionRootSpace(Bones[i], Positions[i]);
                /*}
                else
                {
                    if (i != Positions.Length - 1)
                    {
                        joints[i].targetRotation =
                            Quaternion.FromToRotation(StartDirectionSucc[i], Positions[i + 1] - Positions[i]);
                    }
                    if (!onlyRotate && i != Positions.Length - 1) joints[i].targetPosition = Positions[i];
                }*/
            }
        }
        
        public void SetTarget(Transform trans)
        {
            target = trans;
            Init(Solver);
        }
    }
}