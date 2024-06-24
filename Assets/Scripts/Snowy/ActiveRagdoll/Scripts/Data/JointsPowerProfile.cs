using UnityEngine;

namespace Snowy.ActiveRagdoll
{
    public enum JointsPowerProfileType
    {
        Powerful,
        Balanced,
        Weak,
        Custom
    }
    
    [CreateAssetMenu(fileName = "JointsPowerProfile", menuName = "Snowy/Active Ragdoll/Joints Power Profile")]
    public class JointsPowerProfile : ScriptableObject
    {
        // Drive mode:
        public RotationDriveMode rotationDriveMode = RotationDriveMode.Slerp;
        
        [Header("Powerful")]
        [SerializeField] float powerfulHipsSpring = 1000f;
        [SerializeField] float powerfulHipsDamper = 100f;
        [SerializeField] float powerfulSpring = 1000f;
        [SerializeField] float powerfulDamper = 100f;
        
        [Header("Balanced")]
        [SerializeField] float balancedHipsSpring = 500f;
        [SerializeField] float balancedHipsDamper = 50f;
        [SerializeField] float balancedSpring = 500f;
        [SerializeField] float balancedDamper = 50f;
        
        [Header("Weak")]
        [SerializeField] float weakHipsSpring = 100f;
        [SerializeField] float weakHipsDamper = 10f;
        [SerializeField] float weakSpring = 100f;
        [SerializeField] float weakDamper = 10f;
        
        [Header("Custom")]
        [SerializeField] float customHipsSpring = 500f;
        [SerializeField] float customHipsDamper = 50f;
        [SerializeField] float customSpring = 500f;
        [SerializeField] float customDamper = 50f;
        
        public JointDrive GetJointDrive(JointsPowerProfileType type, bool isHips = false)
        {
            float force = GetSpring(type, isHips);
            float damper = GetDamper(type, isHips);
            JointDrive jointDrive = new JointDrive
            {
                positionSpring = force,
                positionDamper = damper,
                maximumForce = Mathf.Infinity
            };
            return jointDrive;
        }

        public float GetSpring(JointsPowerProfileType type, bool isHips = false)
        {
            switch (type)
            {
                case JointsPowerProfileType.Powerful:
                    return isHips ? powerfulHipsSpring : powerfulSpring;
                case JointsPowerProfileType.Balanced:
                    return isHips ? balancedHipsSpring : balancedSpring;
                case JointsPowerProfileType.Weak:
                    return isHips ? weakHipsSpring : weakSpring;
                case JointsPowerProfileType.Custom:
                    return isHips ? customHipsSpring : customSpring;
                default:
                    return 0f;
            }
        }
        
        public float GetDamper(JointsPowerProfileType type, bool isHips = false)
        {
            switch (type)
            {
                case JointsPowerProfileType.Powerful:
                    return isHips ? powerfulHipsDamper : powerfulDamper;
                case JointsPowerProfileType.Balanced:
                    return isHips ? balancedHipsDamper : balancedDamper;
                case JointsPowerProfileType.Weak:
                    return isHips ? weakHipsDamper : weakDamper;
                case JointsPowerProfileType.Custom:
                    return isHips ? customHipsDamper : customDamper;
                default:
                    return 0f;
            }
        }
    }
}