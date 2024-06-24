using Unity.Collections;
using UnityEngine;

namespace Snowy.FPS.Old
{
    [AddComponentMenu("FPS/Offline/FPSMovementCC"), RequireComponent(typeof(CharacterController))]
    public class FPSMovementCC : FPSMovement
    {
        private CharacterController cc;
        
        [Title("Movement Settings")]
        [SerializeField] private float walkSpeed = 2f;
        [SerializeField] private float runSpeed = 5f;

        [Title("Air & Gravity")] 
        [SerializeField] private float maxJumpHeight = 2f;
        [SerializeField] private float airSpeedMultiplier = 1f;
        [SerializeField] private float gravityMultiplier = 1f;
        
        [Space]
        [Line]
        [SerializeField, ReadOnly] public Vector3 velocity;

        protected override void Awake()
        {
            base.Awake();
            cc = GetComponent<CharacterController>();
        }
        
        protected override void UpdateMovement()
        {
            CheckGrounded();
            Move();
            Jump();
            ApplyGravity();
            ApplyVelocity();
        }

        private void Move()
        {
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }
            
            float speed = isGrounded ? (inputs.sprint ? runSpeed : walkSpeed) : walkSpeed * airSpeedMultiplier;
            velocity = (speed * moveDir) + Vector3.up * velocity.y;
        }

        private void Jump()
        {
            if (inputs.jump && isGrounded)
            {
                velocity.y += Mathf.Sqrt(maxJumpHeight * -2 * Physics.gravity.y);
                OnJumpEvent();
            }
        }

        private void ApplyGravity()
        {
            velocity.y += Physics.gravity.y * Time.deltaTime * gravityMultiplier;
        }

        private void ApplyVelocity()
        {
            cc.Move(velocity * Time.deltaTime);
        }

        public override Vector3 GetVelocity() => cc.velocity;
        

        public override bool IsGrounded() => isGrounded;
    }
}