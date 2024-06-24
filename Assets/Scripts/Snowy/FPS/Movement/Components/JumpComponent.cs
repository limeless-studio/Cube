using System;
using UnityEngine;

namespace Snowy.FPS
{
    [Serializable]
    public class JumpComponent : MovementComponent
    {
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float jumpCooldown = 0.5f;
        
        private float lastJumpTime;

        public override void OnMovementUpdate()
        {
            if (inputs.jump && IsGrounded())
                Jump();
            
        }
        
        private void Jump()
        {
            if (Time.time - lastJumpTime < jumpCooldown)
                return;
            
            // Reset vertical velocity
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            lastJumpTime = Time.time;
        }
    }
}