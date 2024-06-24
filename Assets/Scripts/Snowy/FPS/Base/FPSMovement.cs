using Snowy.Utils.Attributes;
using UnityEngine;
using UnityEngine.Events;

namespace Snowy.FPS.Old
{
    public class FPSMovement : MonoBehaviour, IMovement
    {
        [Title("Debug")]
        [SerializeField] UpdateType updateType = UpdateType.Update;
        [SerializeField] public bool debug;
        [SerializeField, ShowDisabledIf(nameof(debug), true), ReadOnly] protected MovementState state;
        [SerializeField, ShowDisabledIf(nameof(debug), true), ReadOnly] protected Vector3 moveDir;
        [SerializeField, ShowDisabledIf(nameof(debug), true), ReadOnly] protected PlayerInputs inputs;

        [SerializeField, ShowDisabledIf(nameof(debug), true), ReadOnly] protected bool wasGrounded;
        [SerializeField, ShowDisabledIf(nameof(debug), true), ReadOnly] protected bool isGrounded;
        
        [Title("Ground Checker")] 
        [SerializeField] protected LayerMask groundLayer;
        [SerializeField] protected float playerHeight = 2f;
        [SerializeField] protected float groundCheckDistance = 0.1f;

        [Title("Events")]
        public UnityEvent onJump;
        public UnityEvent onLanded;
        protected FPSCharacter Character;
        protected RaycastHit GroundHit;
        
        protected virtual void Awake()
        {
            Character = GetComponentInParent<FPSCharacter>();
            if (Character)
                Character.OnUpdate += OnInputReceived;
            else
            {
                Debug.LogError($"No FPSCharacter found on the Movement GameObject {name}");
                enabled = false;
            }
        }
        
        private void OnInputReceived(ref PlayerInputs input)
        {
            moveDir = transform.right * input.moveDir.x + transform.forward * input.moveDir.y;
            inputs = input;
            inputs.moveDir = moveDir;
            OnUpdate();
        }
        
        protected virtual void OnUpdate()
        {
            if (updateType == UpdateType.Update)
                UpdateMovement();
        }
        
        protected virtual void FixedUpdate()
        {
            if (updateType == UpdateType.FixedUpdate)
                UpdateMovement();
        }
        
        protected virtual void LateUpdate()
        {
            if (updateType == UpdateType.LateUpdate)
                UpdateMovement();
        }

        protected virtual void UpdateMovement()
        {
            CheckGrounded();
        }

        public virtual void CheckGrounded()
        {
            wasGrounded = isGrounded;
            isGrounded = Physics.Raycast(transform.position, Vector3.down, out GroundHit, playerHeight * .5f + 0.1f, groundLayer);
            if (!wasGrounded && isGrounded)
            {
                OnLandedEvent();
            }
            
        }
        
        public virtual Vector3 GetVelocity() => Vector3.zero;

        public virtual bool IsGrounded() => false;
        
        protected void OnJumpEvent() => onJump?.Invoke();
        protected void OnLandedEvent() => onLanded?.Invoke();
        
#if UNITY_EDITOR
        protected virtual void OnDrawGizmosSelected()
        {
            // Draw ground checker
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, Vector3.down * (playerHeight * .5f + groundCheckDistance));
            UnityEditor.Handles.Label(transform.position + Vector3.down * (playerHeight * .5f + 0.2f), "Ground Checker");
        }
#endif
    }
}