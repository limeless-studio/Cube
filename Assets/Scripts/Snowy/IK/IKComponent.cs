using System;
using Snowy.IK;
using Snowy.Utils;
using UnityEngine;

namespace Snowy.ActiveRagdoll.IK
{
    // Here goes all the components that are used by the IKSolver for example: IKChain
    // This way the IKSolver class is not cluttered with all the components and it's easier to manage them.

    public abstract class IKComponent : MonoBehaviour
    {
        // This is the base component for all IK components
        protected IKSolver Solver;
        
        public virtual void Init(IKSolver ikSolver)
        {
            Solver = ikSolver;
        }
        
        public abstract void Solve();
    }

    [Serializable]public class IKBone
    {
        public Transform Bone;
        public float Length;
        public Vector3 Position;
        public Vector3 StartDirection;
        public Quaternion StartRotation;
        
        public void SetPositionWithRespect(Transform root)
        {
            Position = Utilities.GetPositionWithRespect(Bone, root);
        }
        
        public void ApplyPositionWithRespect(Transform root)
        {
            Utilities.SetPositionWithRespect(Bone, Position, root);
        }
    }
}