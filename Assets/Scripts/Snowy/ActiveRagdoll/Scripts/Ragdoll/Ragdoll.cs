using System;
using UnityEngine;

namespace Snowy.ActiveRagdoll.Ragdoll
{
    public enum BalanceMode {
        UprightTorque,
        ManualTorque,
        StabilizerJoint,
        FreezeRotations,
        None,
    }
    
    [Serializable] public class Bone
    {
        public ConfigurableJoint joint;
        public Rigidbody rigidbody;
        public Collider collider;
        public Transform transform;
        private Vector3 m_startPosition;
        private Quaternion m_startRotation;
        
        public void Init()
        {
            if (!transform) return;
            m_startPosition = transform.localPosition;
            m_startRotation = transform.localRotation;
        }
        
        public void Reset()
        {
            if (!transform) return;
            transform.localPosition = m_startPosition;
            transform.localRotation = m_startRotation;
        }
        
        public void SetJointProfile(RotationDriveMode mode, JointDrive drive)
        {
            if (!joint) return;
            joint.rotationDriveMode = mode;
            JointDrive nullDrive = new JointDrive
            {
                positionSpring = 0f,
                positionDamper = 0f,
                maximumForce = Mathf.Infinity
            };
            if (mode == RotationDriveMode.Slerp)
            {
                joint.slerpDrive = nullDrive;
                joint.angularXDrive = nullDrive;
                joint.angularYZDrive = nullDrive;
                joint.slerpDrive = drive;
            }
            else
            {
                joint.slerpDrive = drive;
                joint.angularXDrive = drive;
                joint.angularYZDrive = drive;
                joint.slerpDrive = nullDrive;
            }
        }
    }
    
    [Serializable] public class ThreeChain
    {
        public Bone upper;
        public Bone middle;
        public Bone lower;
    }
    
    [Serializable] public class Armature
    {
        public Bone pelvis;
        public Bone spine;
        public Bone head;
        public ThreeChain[] arms;
        public ThreeChain[] legs;
        
        public void Init()
        {
            if (pelvis != null) pelvis.Init();
            if (spine != null) spine.Init();
            if (head != null) head.Init();
            foreach (var arm in arms)
            {
                if (arm.upper != null) arm.upper.Init();
                if (arm.middle != null) arm.middle.Init();
                if (arm.lower != null) arm.lower.Init();
            }
            foreach (var leg in legs)
            {
                if (leg.upper != null) leg.upper.Init();
                if (leg.middle != null) leg.middle.Init();
                if (leg.lower != null) leg.lower.Init();
            }
        }
        
        public void SetJointProfile(JointsPowerProfile profile, JointsPowerProfileType type = JointsPowerProfileType.Balanced)
        {
            var mode = profile.rotationDriveMode;
            var pelvisDrive = profile.GetJointDrive(type, true);
            var drive = profile.GetJointDrive(type);
            pelvis?.SetJointProfile(mode, pelvisDrive);
            spine?.SetJointProfile(mode, drive);
            head?.SetJointProfile(mode, drive);

            foreach (var arm in arms)
            {
                arm.upper?.SetJointProfile(mode, drive);
                arm.middle?.SetJointProfile(mode, drive);
                arm.lower?.SetJointProfile(mode, drive);
            }
            
            foreach (var leg in legs)
            {
                leg.upper?.SetJointProfile(mode, drive);
                leg.middle?.SetJointProfile(mode, drive);
                leg.lower?.SetJointProfile(mode, drive);
            }
        }
    }
}