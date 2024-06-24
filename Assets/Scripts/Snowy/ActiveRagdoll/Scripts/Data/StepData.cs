using UnityEngine;

namespace Snowy.ActiveRagdoll.Data
{

    [CreateAssetMenu(fileName = "StepData", menuName = "Snowy/Active Ragdoll/StepData", order = 0)]
    public class StepData : ScriptableObject
    {
        public AnimationCurve upperLegCurve;
        public AnimationCurve lowerLegCurve;
        public float stepLength; 
        public float stepDuration;
        public float stepMultiplier;
    }
}