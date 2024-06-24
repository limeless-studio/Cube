using System.Collections.Generic;
using Snowy.Utils.Attributes;
using UnityEngine;
using UnityEngine.Events;

namespace Snowy.FPS
{
    
    [AddComponentMenu("FPS/Offline/FPSMovement"), RequireComponent(typeof(Rigidbody))]
    public class FPSMovement : MonoBehaviour
    {
        [Title("Debug")]
        [SerializeField] UpdateType updateType = UpdateType.Update;
        [SerializeField] public bool debug;
        [SerializeField, ShowDisabledIf(nameof(debug), true), ReadOnly] protected MovementState state;
        [SerializeField, ShowDisabledIf(nameof(debug), true), ReadOnly] protected Vector3 moveDir;
        [SerializeField, ShowDisabledIf(nameof(debug), true), ReadOnly] protected PlayerInputs inputs;

        [SerializeField, ShowDisabledIf(nameof(debug), true), ReadOnly] protected bool wasGrounded;
        [SerializeField, ShowDisabledIf(nameof(debug), true), ReadOnly] protected bool isGrounded;
        [SerializeReference, ShowDisabledIf(nameof(debug), true)] List<MovementComponent> components = new();
        
        [Title("Ground Checker")] 
        [SerializeField] protected LayerMask groundLayer;
        [SerializeField] protected float groundCheckDistance = 0.1f;

        [Title("Air & Gravity")] 
        [SerializeField] private float groundDrag = 6f;
        
        [Title("Events")]
        public UnityEvent onLanded;
        protected RaycastHit groundHit;
        
        private FPSCharacter character;
        private Rigidbody rb;

        public FPSCharacter Character
        {
            get
            {
                if (character == null)
                    character = GetComponentInParent<FPSCharacter>();
                return character;
            }
        }
        public Rigidbody Rb
        {
            get
            {
                if (rb == null)
                    rb = GetComponent<Rigidbody>();
                return rb;
            }
        }
        
        
        
        protected void Awake()
        {
            character = GetComponentInParent<FPSCharacter>();
            if (character)
                character.OnUpdate += OnInputReceived;
            else
            {
                Debug.LogError($"No FPSCharacter found on the Movement GameObject {name}");
                enabled = false;
            }
            
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;
            
            // Initialize components
            foreach (var component in components)
            {
                component.Initialize(this);
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
            CheckGrounded();

            foreach (var component in components)
                component.OnUpdate(ref inputs);
            
            if (updateType == UpdateType.Update)
                UpdateMovement();
            
            // CHECK DEATH
            if (transform.position.y < -20)
            {
                Debug.Log("Player fell off the map");
                character.SendTakeDamage(100);
            }
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
            foreach (var component in components)
                component.OnMovementUpdate();
            
            // Rotate the player to the direction of the camera
            if (character.Camera)
            {
                var yRot = character.Camera.GetYRotation();
                Rb.MoveRotation(Quaternion.Euler(0, yRot, 0));
            }
        }
        
        public virtual void CheckGrounded()
        {
            wasGrounded = isGrounded;
            var height = character.GetPlayerHeight();
            isGrounded = Physics.Raycast(transform.position, Vector3.down, out groundHit, height * .5f + 0.1f, groundLayer);
            if (!wasGrounded && isGrounded)
            {
                onLanded?.Invoke();
            }
            
            Rb.linearDamping = isGrounded ? groundDrag : 0f;
        }
        
        public Rigidbody GetRigidbody() => Rb;

        public Vector3 GetVelocity() => Rb.linearVelocity;

        public bool IsGrounded() => isGrounded;
        
        public LayerMask GetGroundLayer() => groundLayer;
        
        public RaycastHit GetGroundHit() => groundHit;

        public bool OnSlope(float slopeAngleLimit = 45f)
        {
            var hit = GetGroundHit();
            var angle = Vector3.Angle(Vector3.up, hit.normal);
            return angle < slopeAngleLimit && angle != 0;
        }

        public Vector3 GetSlopeDirection(Vector3 dir)
        {
            var hit = GetGroundHit();
            return Vector3.ProjectOnPlane(dir, hit.normal).normalized;
        }
        
        public void SetHeight(float height) => Character.SetPlayerHeight(height);
        
        public float GetHeight() => Character.GetPlayerHeight();
        
        public void ResetHeight() => Character.ResetHeight();

        
        #region component API
        
        public void AddComponent(MovementComponent component)
        {
            component.Initialize(this);
            components.Add(component);
        }
        
        public void RemoveComponent(MovementComponent component)
        {
            components.Remove(component);
        }
        
        public MovementComponent GetComponentAtIndex(int index)
        {
            return components[index];
        }
        
        public void RemoveComponentAt(int index)
        {
            components.RemoveAt(index);
        }
        
        public T GetMovementComponent<T>() where T : MovementComponent
        {
            foreach (var component in components)
            {
                if (component is T t)
                    return t;
            }

            return null;
        }
        
        public T[] GetMovementComponents<T>() where T : MovementComponent
        {
            var list = new List<T>();
            foreach (var component in components)
            {
                if (component is T t)
                    list.Add(t);
            }

            return list.ToArray();
        }
        
        public MovementComponent[] GetComponents()
        {
            return components.ToArray();
        }
        
        #endregion
        
#if UNITY_EDITOR
        
        private void OnGUI()
        {
            // Draw debug texts
            var stateStr = state.ToString();
            var rbSpeed = (int)rb.linearVelocity.magnitude + " m/s";
            var velocity = GetVelocity().ToString("F2") + " m/s";
            var grounded = isGrounded ? "Yes" : "No";
            GUI.Label(new Rect(10, 10, 200, 20), $"State: {stateStr}");
            GUI.Label(new Rect(10, 30, 200, 20), $"Speed: {rbSpeed}");
            GUI.Label(new Rect(10, 50, 200, 20), $"Velocity: {velocity}");
            GUI.Label(new Rect(10, 70, 200, 20), $"Grounded: {grounded}");
        }
        
        protected virtual void OnDrawGizmosSelected()
        {
            // Draw ground checker
            Gizmos.color = Color.green;
            var playerHeight = Character.GetPlayerHeight();
            Gizmos.DrawRay(transform.position, Vector3.down * (playerHeight * .5f + groundCheckDistance));
            UnityEditor.Handles.Label(transform.position + Vector3.down * (playerHeight * .5f + 0.2f), "Ground Checker");
        }
        
#endif
    }
}