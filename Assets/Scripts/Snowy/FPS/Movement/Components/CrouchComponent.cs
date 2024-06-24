using System;
using UnityEngine;

namespace Snowy.FPS
{
    [Serializable]
    public class CrouchComponent : MovementComponent
    {
        [SerializeField] float crouchHeight = 0.5f;
        [SerializeField] float speedMultiplier = 0.5f;
        
        private WalkComponent walkComponent;
        private bool wasCrouching;
        
        public override void Initialize(FPSMovement parent)
        {
            base.Initialize(parent);
            walkComponent = movement.GetMovementComponent<WalkComponent>();
        }
        
        public override void OnUpdate(ref PlayerInputs @in)
        {
            base.OnUpdate(ref @in);

            if (@in.crouch && !wasCrouching)
            {
                Crouch();
            } else if (!@in.crouch && wasCrouching)
            {
                Stand();
            }
            
            wasCrouching = @in.crouch;
        }

        public override void OnMovementUpdate() { /* ignore */ }
        
        private void Crouch()
        {
            AddForce(Vector3.down, ForceMode.Impulse);
            movement.SetHeight(crouchHeight);
            walkComponent.SetSpeedMultiplier(speedMultiplier);
        }
        
        private void Stand()
        {
            AddForce(Vector3.up, ForceMode.Impulse);
            movement.ResetHeight();
            walkComponent.SetSpeedMultiplier(1f);
        }
    }
}