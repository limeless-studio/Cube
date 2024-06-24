using System;
using UnityEngine;

namespace Snowy.FPS
{
    /// <summary>
    /// It helps to solve the step problem with rigibody so that the player can walk on stairs.
    /// </summary>
    [Serializable]
    public class StepSolverComponent : MovementComponent
    {
        [SerializeField] private Transform lowerRay;
        [SerializeField] private Transform upperRay;
        [SerializeField] private float stepHeight = 0.5f;
        [SerializeField] private float minStepDepth = 0.1f;
        [SerializeField] private float stepDistance = 0.5f;
        [SerializeField] private float stepSmooth = 0.5f;

        public override void Initialize(FPSMovement parent)
        {
            base.Initialize(parent);
            if (upperRay && lowerRay) upperRay.transform.position = lowerRay.transform.position + Vector3.up * stepHeight;
        }

        public override void OnMovementUpdate()
        {
            // if player is moving
            if (inputs.moveDir != Vector3.zero)
            {
                if (DetectStep(out RaycastHit _))
                {
                    // step
                    Step();
                }
            }
        }
        
        private bool DetectStep(out RaycastHit hit)
        {
            RaycastHit upperHit;
            var direction = inputs.moveDir.normalized;
            if (Physics.Raycast(lowerRay.position, direction, out hit, stepHeight + 0.1f, movement.GetGroundLayer()))
            {
                if (Physics.Raycast(upperRay.position, direction, out upperHit, stepHeight + 0.1f, movement.GetGroundLayer()))
                {
                    // check the depth of the step
                    if (upperHit.distance - hit.distance >= minStepDepth)
                    {
                        return true;
                    }

                    return false;
                }
                
                // it means that the upper ray didn't hit anything, so we can just step
                return true;
            }
            
            return false;
        }

        private void Step()
        {
            rb.position += Vector3.up * stepSmooth;
        }
    }
}