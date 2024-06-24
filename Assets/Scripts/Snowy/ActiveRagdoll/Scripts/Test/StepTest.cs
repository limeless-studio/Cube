using Snowy.ActiveRagdoll.Ragdoll;
using UnityEngine;

namespace Snowy.ActiveRagdoll.Test
{
    public class StepTest : MonoBehaviour
    {
        [SerializeField] private RagdollAnimator animator;
        
        [SerializeField] private bool step;
        
        private void Update()
        {
            if (step)
            {
                animator.Step();
            }
        }
    }
}