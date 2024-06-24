using UnityEngine;

namespace Snowy.FPS
{
    public class WalkComponent : MovementComponent
    {
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField, ShowIf(nameof(sprintEnabled), true)] private float sprintSpeed = 10f;
        [SerializeField] private float airMultiplier = 0.5f;
        [SerializeField] private float counterMovement = 0.175f;
        [SerializeField] private bool sprintEnabled = true;
        [SerializeField] private bool accountForSlope = true;
        
        private float speedMultiplier = 1f;
        
        public override void OnMovementUpdate()
        {
            float speed = (inputs.sprint && sprintEnabled ? sprintSpeed : walkSpeed) * speedMultiplier * 10f;
            
            if (accountForSlope && movement.OnSlope())
            {
                var slopeDirection = movement.GetSlopeDirection(inputs.moveDir);
                AddForce(slopeDirection * (speed * 2f));

                if (rb.linearVelocity.y > 0)
                    AddForce(Vector3.down * 80f);
            }
            else
            {
                Vector3 move = inputs.moveDir * speed;
                AddForce(move * (IsGrounded() ? 1f : airMultiplier));
            }

            CounterMovement();
        }

        private void CounterMovement()
        {
            
            Vector3 vel = rb.linearVelocity;
            Vector3 horizontalVel = new Vector3(vel.x, 0, vel.z);

            float maxSpeed = (inputs.sprint && sprintEnabled ? sprintSpeed : walkSpeed) * speedMultiplier;

            if (inputs.moveDir == Vector3.zero)
            {
                // Apply counter movement when there's no input
                // Vector3 counterForce = -horizontalVel * counterMovement;
                // AddForce(counterForce, ForceMode.Acceleration);
            }
            else
            {
                // If the player is moving, apply a counter force to limit the speed to the max speed
                if (horizontalVel.magnitude > maxSpeed)
                {
                    Vector3 limitedForce = -horizontalVel.normalized * maxSpeed * counterMovement;
                    AddForce(limitedForce, ForceMode.Acceleration);
                }
            }
        }
        
        public void SetSpeedMultiplier(float multiplier)
        {
            speedMultiplier = multiplier;
        }
    }
}