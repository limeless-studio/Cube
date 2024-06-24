using System;
using UnityEngine;

namespace Snowy.FPS
{
    // TODO: MOVEMENT COMPONENT PROPERTY DRAWER.
    [Serializable] public abstract class MovementComponent
    {
        protected FPSMovement movement;
        protected Rigidbody rb;
        protected PlayerInputs inputs;

        
        public virtual void Initialize(FPSMovement parent)
        {
            movement = parent;
            rb = parent.GetRigidbody();
        }

        public virtual void OnUpdate(ref PlayerInputs @in)
        {
            inputs = @in;
        }
        
        public abstract void OnMovementUpdate();

        protected void AddForce(Vector3 force, ForceMode mode = ForceMode.Force)
        {
            rb.AddForce(force, mode);
        }
        
        protected bool IsGrounded()
        {
            return movement.IsGrounded();
        }
    }
}