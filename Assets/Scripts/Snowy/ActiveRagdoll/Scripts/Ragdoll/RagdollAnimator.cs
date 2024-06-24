using System.Collections;
using Snowy.ActiveRagdoll.Data;
using Snowy.SnInput;
using UnityEngine;

namespace Snowy.ActiveRagdoll.Ragdoll
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Snowy/Active Ragdoll/Ragdoll Animator")]
    [RequireComponent(typeof(Animator))]
    public class RagdollAnimator : MonoBehaviour
    {
        # region VARS: References
        [Tooltip("The animator to be to setup this ragdoll.")]
        public Animator animator;

        [Tooltip("The armature to be used for the ragdoll.")]
        public Armature armature;
        [Tooltip("Joints Power Profile to be used for the ragdoll.")]
        [SerializeField] JointsPowerProfile jointsPowerProfile;
        
        # endregion
        
        # region VARS: Balance Settings
        [Tooltip("The balance mode to be used to keep the ragdoll from falling.")]
        [SerializeField] BalanceMode balanceMode;
        
        // Upright Torque
        [SerializeField] float uprightTorque = 10000;
        [Tooltip("Defines how much torque percent is applied given the inclination angle percent [0, 1]")]
        [SerializeField] AnimationCurve uprightTorqueFunction;
        [SerializeField] float rotationTorque = 500;
        
        // Manual Torque
        [SerializeField] float manualTorque = 500;
        [SerializeField] float maxManualRotSpeed  = 5;
        [SerializeField] Vector2 torqueInput;
        
        // Freeze Rotations
        [SerializeField] float freezeRotationSpeed = 5f;
        
        # endregion
        
        # region VARS: Motion Settings
        
        [Tooltip("The preset to be used for the ragdoll legs.")]
        [SerializeField] StepData stepInfo;
        [Tooltip("The preset to be used for the ragdoll arms.")]
        [SerializeField] ArmsData armsInfo;
        [Tooltip("The force to be applied to the hips to move the character forward.")]
        [SerializeField] float hipsForwardForce = 10f;
        
        # endregion
        
        # region VARS: Joints Settings
        
        [Tooltip("Current power profile type.")]
        [SerializeField] JointsPowerProfileType jointsPowerProfileType;
        [SerializeField] bool setupOnStart = true;
        
        # endregion
        
        # region VARS: Private General
        
        public Vector3 TargetDirection { get; set; }
        private Quaternion m_targetRotation;
        private BalanceMode prevBalanceMode = BalanceMode.None;
        
        private bool isStepping;
        
        # endregion
        
        #region Unity Editor
        # if UNITY_EDITOR
        private void OnValidate()
        {
            if (animator == null)
                animator = GetComponent<Animator>();
        }
        # endif
        #endregion
        
        #region Public Methods

        public void SetupBones()
        {
            if (animator == null)
            {
                Debug.LogError("Animator is not assigned.");
                return;
            }
            
            var pelvis = animator.GetBoneTransform(HumanBodyBones.Hips);

            var leftHips = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            var leftKnee = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            var leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            
            var rightHips = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            var rightKnee = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            var rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
            
            var leftArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            var leftElbow = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);

            var rightArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            var rightElbow = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            
            var middleSpine = animator.GetBoneTransform(HumanBodyBones.Spine);
            var head = animator.GetBoneTransform(HumanBodyBones.Head);
            
            armature = new Armature {
                pelvis = GetBone(pelvis),
                spine = GetBone(middleSpine),
                head = GetBone(head),
                arms = new[] {
                    new ThreeChain
                    {
                        upper = GetBone(leftArm),
                        middle = GetBone(leftElbow),
                        lower = null
                    },
                    new ThreeChain
                    {
                        upper = GetBone(rightArm),
                        middle = GetBone(rightElbow),
                        lower = null
                    }
                },
                legs = new[] {
                    new ThreeChain
                    {
                        upper = GetBone(leftHips),
                        middle = GetBone(leftKnee),
                        lower = GetBone(leftFoot)
                    },
                    new ThreeChain
                    {
                        upper = GetBone(rightHips),
                        middle = GetBone(rightKnee),
                        lower = GetBone(rightFoot)
                    }
                }
            };
        }

        public Bone GetBone(Transform local)
        {
            return new Bone
            {
                collider = local.GetComponent<Collider>(),
                joint = local.GetComponent<ConfigurableJoint>(),
                rigidbody = local.GetComponent<Rigidbody>(),
                transform = local,
            };
        }
        
        public void SetPowerProfileType(JointsPowerProfileType type)
        {
            jointsPowerProfileType = type;
            armature.SetJointProfile(jointsPowerProfile, jointsPowerProfileType);
        }
        
        #endregion
        
        # region Balancer

        public void SetMode(BalanceMode mode)
        {
            balanceMode = mode;
            var hips = armature.pelvis;
            hips.rigidbody.constraints = RigidbodyConstraints.None;
            switch (balanceMode)
            {
                case BalanceMode.UprightTorque:
                    break;
                case BalanceMode.ManualTorque:
                    break;
                case BalanceMode.StabilizerJoint:
                    break;
                case BalanceMode.FreezeRotations:
                    hips.rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
                    break;
                case BalanceMode.None:
                    break;
            }
        }
        private void Balance()
        {
            if (prevBalanceMode != balanceMode)
            {
                SetMode(balanceMode);
            }
            
            UpdateTargetRotation();
            switch (balanceMode)
            {
                case BalanceMode.UprightTorque:
                    UprightTorque();
                    break;
                case BalanceMode.ManualTorque:
                    torqueInput = InputManager.Instance.MoveInput;
                    ManualTorque();
                    break;
                case BalanceMode.StabilizerJoint:
                    StabilizerJoint();
                    break;
                case BalanceMode.FreezeRotations:
                    FreezeRotations();
                    break;
                case BalanceMode.None:
                    break;
            }
        }
        
        private void UpdateTargetRotation() {
            if (TargetDirection != Vector3.zero)
                m_targetRotation = Quaternion.LookRotation(TargetDirection, Vector3.up);
            else
                m_targetRotation = Quaternion.identity;
        }

        private void UprightTorque()
        {
            var hips = armature.pelvis;
            var balancePercent = Vector3.Angle(hips.transform.up, Vector3.up) / 180f;
            balancePercent = uprightTorqueFunction.Evaluate(balancePercent);
            var rot = Quaternion.FromToRotation(hips.transform.up, Vector3.up).normalized;
            
            hips.rigidbody.AddTorque(new Vector3(rot.x, rot.y, rot.z) * (uprightTorque * balancePercent));
            
            var directionAnglePercent = Vector3.SignedAngle(hips.transform.forward, TargetDirection, Vector3.up) / 180f;
            hips.rigidbody.AddRelativeForce(0, directionAnglePercent * rotationTorque, 0);
        }
        private void ManualTorque()
        {
            var hips = armature.pelvis;
            if (hips.rigidbody.angularVelocity.magnitude < maxManualRotSpeed) {
                var force = torqueInput * manualTorque;
                hips.rigidbody.AddRelativeTorque(force.y, 0, force.x);
            }
        }
        
        private void StabilizerJoint()
        {
            
        }
        
        private void FreezeRotations()
        {
            var hips = armature.pelvis;
            var smoothedRot = Quaternion.Lerp(hips.transform.rotation, m_targetRotation,
                Time.fixedDeltaTime * freezeRotationSpeed);
            hips.rigidbody.MoveRotation(smoothedRot);
        }
        
        # endregion
        
        # region Motion
        
        public void Step()
        {
            if (!isStepping)
            {
                var leftLeg = armature.legs[0];
                var rightLeg = armature.legs[1];
                
                var leg = GetBackLeg(leftLeg, rightLeg);
                StartCoroutine(StepCoroutine(leg, stepInfo));
            }
        }
        
        private ThreeChain GetBackLeg(ThreeChain leftLeg, ThreeChain rightLeg)
        {
            var hipsPos = armature.pelvis.transform.position;
            var leftLegPos = leftLeg.lower.transform.position;
            var rightLegPos = rightLeg.lower.transform.position;
            var leftLegToHips = leftLegPos - hipsPos;
            var rightLegToHips = rightLegPos - hipsPos;
            var forward = armature.pelvis.transform.forward;
            var leftAngle = Vector3.Angle(forward, leftLegToHips);
            var rightAngle = Vector3.Angle(forward, rightLegToHips);
            
            return leftAngle > rightAngle ? rightLeg : leftLeg;
        }
        
        private IEnumerator StepCoroutine(ThreeChain leg, StepData step)
        {
            isStepping = true;
            var hips = armature.pelvis;
            var start = leg.lower.transform.position;
            var end = start + leg.lower.transform.forward * step.stepLength;
            var time = 0f;
            var stepDuration = step.stepDuration;
            var stepMultiplier = step.stepMultiplier;
            var upperLegCurve = step.upperLegCurve;
            var lowerLegCurve = step.lowerLegCurve;
             
            while (time < step.stepDuration)
            {
                time += Time.deltaTime;
                var upperLegValue = upperLegCurve.Evaluate(time / stepDuration) * stepMultiplier;
                var lowerLegValue = lowerLegCurve.Evaluate(time / stepDuration) * stepMultiplier;
                
                leg.upper.joint.targetRotation = Quaternion.Euler(upperLegValue, 0, 0);
                leg.middle.joint.targetRotation = Quaternion.Euler(lowerLegValue, 0, 0);
                hips.rigidbody.AddForce(hips.transform.forward * hipsForwardForce);
                
                Debug.Log("Stepping");
                //leg.rigidbody.MovePosition(Vector3.Lerp(start, end, time / stepDuration));
                
                yield return null;
            }
            isStepping = false;
        }
        
        #endregion
        
        #region Unity Methods

        void Start()
        {
            if (setupOnStart) SetupBones();
            armature.Init();
            
            if (jointsPowerProfile == null)
                Debug.LogError("Joints Power Profile is not assigned.");
            
            SetPowerProfileType(jointsPowerProfileType);
            SetMode(balanceMode);
        }
        
        private void FixedUpdate()
        {
            // Balance
            Balance();
        }
        
        #endregion
    }
}