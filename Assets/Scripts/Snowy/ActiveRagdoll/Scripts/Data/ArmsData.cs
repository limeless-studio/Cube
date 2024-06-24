using System;
using UnityEngine;

namespace Snowy.ActiveRagdoll.Data
{
    public enum ArmState
    {
        Idle,
        PickUp,
        Punch,
        PunchThrow
    }
    
    [Serializable] public class ArmSettings
    {
        public Vector3 upperTargetVel;
        public Vector3 lowerTargetVel;
        public float rotationSpring = 100;
        public float smoothTime = 0.1f;
    }
    
    [CreateAssetMenu(fileName = "ArmData", menuName = "Snowy/Active Ragdoll/ArmData", order = 0)]
    public class ArmsData : ScriptableObject
    {
        // Left Arm
        public ArmSettings idleLeft = new ArmSettings();
        public ArmSettings pickUpLeft = new ArmSettings();
        public ArmSettings punchLeft = new ArmSettings();
        public ArmSettings punchThrowLeft = new ArmSettings();
        
        // Right Arm
        public ArmSettings idleRight = new ArmSettings();
        public ArmSettings pickUpRight = new ArmSettings();
        public ArmSettings punchRight = new ArmSettings();
        public ArmSettings punchThrowRight = new ArmSettings();
    }
}