/*using System.Collections;
using UnityEngine;

namespace Snowy.FPS
{
    
    [AddComponentMenu("FPS/Offline/FPSMovement"), RequireComponent(typeof(Rigidbody))]
    public class FPSMovementOLD : FPSMovement
    {
        private Rigidbody rb;
        
        [Title("Movement Settings")]
        [SerializeField] private CounterMovementType counterMovementType = CounterMovementType.None;
        [SerializeField] private float walkSpeed = 40f;
        [SerializeField] private float sprintSpeed = 70f;
        [SerializeField] private float crouchSpeed = 20f;
        [SerializeField] private float groundDrag = 6f;
        [SerializeField] private float speedIncreaseMultiplier;
        [SerializeField] private float slopeIncreaseMultiplier;
        
        [Title("Crouch & Slide")]
        [SerializeField] private float crouchYScale = .5f;
        [SerializeField] private float slidingForce = 10f;
        [SerializeField] private float slideMaxTime = 1f;

        [Title("Air & Gravity")] 
        [SerializeField] private float maxSlopeAngle = 45f;
        [SerializeField] private float maxJumpHeight = 2f;
        [SerializeField] private float jumpDelay = .1f;
        [SerializeField] private float airSpeedMultiplier = 1f;
        [SerializeField] private float additionalGravity = 1f;
        
        private float m_originalYScale;
        private float m_desiredMoveSpeed;
        private float m_lastDesiredMoveSpeed;
        private float m_slideSpeed;
        private bool m_isSliding;
        private bool m_isJumping;
        private bool m_canJump = true;
        private bool m_canSlide = true;
        private bool m_exitSlope;
        private bool m_onSlope;
        private bool m_wasCrouching;
        
        protected override void Awake()
        {
            base.Awake();
            rb = GetComponent<Rigidbody>();
            m_originalYScale = transform.localScale.y;
            rb.freezeRotation = true;

            onLanded.AddListener(OnLanded);
        }

        protected override void Update()
        {
            CheckGrounded();
            CrouchAndSlide();
            CounterMovement();
            base.Update();
        }

        protected override void UpdateMovement()
        {
            CheckJump();
            Move();
            ApplyGravity();
            StateHandler();
        }
        
        private void StateHandler()
        {
            // Mode - Crouching / Sliding
            if (isGrounded && inputs.crouch && !m_isJumping)
            {
                // Mode - Crouching
                if (isGrounded && !m_isSliding)
                {
                    state = MovementState.Crouching;
                    m_desiredMoveSpeed = crouchSpeed;
                }
                
                // Mode - Sliding
                else if (m_isSliding)
                {
                    state = MovementState.Sliding;
                    m_desiredMoveSpeed = m_onSlope && GetVelocity().y < 0 ? slidingForce : m_slideSpeed;
                }
            }
            // Mode - Idle
            else if (isGrounded && moveDir == Vector3.zero)
            {
                state = MovementState.Idle;
            }
            // Mode - Sprinting
            else if (isGrounded && inputs.sprint)
            {
                state = MovementState.Sprinting;
                m_desiredMoveSpeed = sprintSpeed;
            }
            // Mode - Walking
            else if (isGrounded)
            {
                state = MovementState.Walking;
                m_desiredMoveSpeed = walkSpeed;
            }
            // Mode - Falling
            else
            {
                state = MovementState.Falling;
            }
            
            float diff = Mathf.Abs(m_desiredMoveSpeed - m_lastDesiredMoveSpeed);
            if (diff > 4f && moveSpeed != 0)
            {
                StopCoroutine(nameof(LerpMoveSpeed));
                StartCoroutine(LerpMoveSpeed());
            } 
            else
            {
                moveSpeed = m_desiredMoveSpeed;
            }
            
            m_lastDesiredMoveSpeed = m_desiredMoveSpeed;
        }

        IEnumerator LerpMoveSpeed()
        {
            // smoothly lerp movementSpeed to desired value
            float time = 0;
            float difference = Mathf.Abs(m_desiredMoveSpeed - moveSpeed);
            float startValue = moveSpeed;

            while (time < difference)
            {
                moveSpeed = Mathf.Lerp(startValue, m_desiredMoveSpeed, time / difference);

                if (OnSlope())
                {
                    float slopeAngle = Vector3.Angle(Vector3.up, GroundHit.normal);
                    float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                    time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
                }
                else
                    time += Time.deltaTime * speedIncreaseMultiplier;

                yield return null;
            }

            moveSpeed = m_desiredMoveSpeed;
        }
        
        public override void CheckGrounded()
        {
            base.CheckGrounded();
            if (isGrounded) rb.linearDamping = groundDrag;
            else rb.linearDamping = 0;
        } 

        private void Move()
        {
            m_onSlope = OnSlope();
            rb.useGravity = !m_onSlope;
            
            // On Slope
            if (m_onSlope && !m_exitSlope)
            {
                rb.AddForce(GetSlopeDirection(moveDir) * (moveSpeed * 20f), ForceMode.Force); 
                
                // Hold player to the slope
                if (GetVelocity().y > 0)
                    rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
            
            // On Ground
            else if (isGrounded)
                rb.AddForce(moveDir * (moveSpeed * 10f), ForceMode.Force);
            
            // In Air
            else 
                rb.AddForce(moveDir * (moveSpeed * 10f * airSpeedMultiplier), ForceMode.Force);
        }
        
        private bool CheckJump(bool disableSlide = false)
        {
            if (inputs.jump && isGrounded && m_canJump)
            {
                if (!UnCrouch(crouchYScale)) return false;
                if (disableSlide) m_canSlide = false;
                // Reset downward velocity
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                Jump();
                
                return true;
            }
            
            return false;
        }

        private void Jump()
        {
            m_onSlope = false;
            m_exitSlope = true;
            m_isJumping = true;
            inputs.crouch = false;
            m_wasCrouching = false;
            rb.AddForce(Vector3.up * Mathf.Sqrt(maxJumpHeight * -2 * Physics.gravity.y), ForceMode.Impulse);
            OnJumpEvent();
            m_canJump = false;
            StartCoroutine(ResetJump());
        }
        
        private void CrouchAndSlide()
        {
            if (inputs.crouch && !m_wasCrouching)
            {
                // Check if the player is moving then slide
                if (moveDir != Vector3.zero && m_canSlide)
                {
                    m_slideSpeed = inputs.sprint ? sprintSpeed : walkSpeed;
                    if (!isGrounded) m_slideSpeed = slidingForce;
                    Crouch();
                    StartCoroutine(Slide());
                } else if (isGrounded && !m_isJumping) Crouch();
            }
            else if (!inputs.crouch && m_wasCrouching && isGrounded)
            {
                // Stop sliding
                if (m_isSliding)
                {
                    m_isSliding = false;
                    StopCoroutine(nameof(Slide));
                }
                UnCrouch(crouchYScale);
            }
        }

        private void Crouch()
        {
            StopCoroutine(nameof(ScalePlayerY));
            StartCoroutine(ScalePlayerY(m_originalYScale * crouchYScale));
            m_wasCrouching = true;
        }

        private bool UnCrouch(float scale)
        {
            float height = scale * m_originalYScale;
            StopCoroutine(nameof(Slide));

            // Check if there is a ceiling above the player
            if (Physics.Raycast(transform.position, Vector3.up, out _, (height * 2) + .2f))
            {
                m_wasCrouching = true;
                inputs.crouch = true;
                moveSpeed = crouchSpeed;
                return false;
            }

            // Move the player up
            // transform.position += Vector3.up * height;
            m_wasCrouching = false;
            if (Mathf.Approximately(transform.localScale.y, m_originalYScale)) return true;
            StopCoroutine(nameof(ScalePlayerY));
            StartCoroutine(ScalePlayerY(m_originalYScale));
            return true;
        }
        
        private bool OnSlope()
        {
            if (GroundHit.collider != null)
            {
                float angle = Vector3.Angle(Vector3.up, GroundHit.normal);
                return angle < maxSlopeAngle && angle != 0;
            }
            return false;
        }
        
        private Vector3 GetSlopeDirection(Vector3 direction)
        {
            return Vector3.ProjectOnPlane(direction, GroundHit.normal).normalized;
        }
        
        private void CounterMovement()
        {
            Vector3 vel = GetVelocity();
            Vector3 flatVel = new Vector3(vel.x, 0, vel.z);
            float limit = moveSpeed;
            
            // Slop
            if (m_onSlope && !m_exitSlope)
            {
                if (vel.magnitude > limit)
                    rb.linearVelocity = vel.normalized * limit;
            }
            else {
                m_exitSlope = false;
                switch (counterMovementType)
                {
                    case CounterMovementType.Friction:
                        float currentSpeed = flatVel.magnitude;
                        float coeff = currentSpeed > limit ? 0 : 1 - currentSpeed / limit;
                        rb.AddForce(-flatVel.normalized * coeff, ForceMode.Acceleration);
                        break;
                    case CounterMovementType.VelocityLimitHard:
                        if (flatVel.magnitude > limit)
                        {
                            Vector3 limitVel = flatVel.normalized * limit;
                            rb.linearVelocity = new Vector3(limitVel.x, vel.y, limitVel.z);
                        }
                        break;
                    case CounterMovementType.VelocityLimitLerp:
                        float limitSoftLerp = limit * .9f;
                        if (flatVel.magnitude > limitSoftLerp)
                        {
                            Vector3 limitVel = flatVel.normalized * limit;
                            rb.linearVelocity = Vector3.Lerp(vel, new Vector3(limitVel.x, vel.y, limitVel.z), Time.deltaTime * 10);
                        }
                        break;
                    case CounterMovementType.VelocityLimitForce:
                        float limitSoft = limit * .9f;
                        if (flatVel.magnitude > limitSoft)
                        {
                            rb.AddForce(-flatVel.normalized * 10, ForceMode.Acceleration);
                        }
                        break;
                }
            }
        }
        
        private void ApplyGravity()
        {
            if (!isGrounded)
            {
                rb.AddForce(Vector3.down * additionalGravity, ForceMode.Acceleration);
            }
        }
        
        private IEnumerator Slide()
        {
            m_isSliding = true;
            float time = 0;
            while (time < slideMaxTime)
            {
                // if the player is jumping, stop sliding and Jump
                if (CheckJump(true))
                {
                    yield break;
                }
                
                // Stick the player to the ground at the same time slide
                Vector3 dir = Vector3.down * 20f;
                float speed = m_onSlope && GetVelocity().y < 0 ? slidingForce : m_slideSpeed;
                if (m_onSlope)
                    rb.AddForce(GetSlopeDirection(moveDir) * speed + dir, ForceMode.Force);
                else
                    rb.AddForce(moveDir * speed + dir, ForceMode.Force);
                
                time += Time.deltaTime;
                yield return null;
            }
            
            // Reset the down force
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, -2f, rb.linearVelocity.z);
            
            m_isSliding = false;
        }
        
        IEnumerator ScalePlayerY(float scale)
        {
            float time = 0;
            var duration = .2f;
            var startScale = transform.localScale.y;
            float finalDuration = duration * (Mathf.Abs(scale - startScale) / m_originalYScale);
            float dir = scale > startScale ? 1 : -1;
            
            while (time < finalDuration)
            {
                float newY = Mathf.Lerp(startScale, scale, time / finalDuration);
                transform.localScale = new Vector3(transform.localScale.x, newY, transform.localScale.z);
                time += Time.deltaTime;
                rb.AddForce(Vector3.up * (5f * dir), ForceMode.Force);
                yield return null;
            }
        }
        
        IEnumerator ResetJump()
        {
            yield return new WaitForSeconds(jumpDelay);
            m_canJump = true;
            m_exitSlope = false;
            m_isSliding = false;
            m_canSlide = true;
        }

        private void OnLanded()
        {
            m_isJumping = false;
        }

        public override Vector3 GetVelocity() => rb.linearVelocity;

        public override bool IsGrounded() => isGrounded;
        
#if UNITY_EDITOR

        private void OnGUI()
        {
            // Draw debug texts
            var stateStr = state.ToString();
            var rbSpeed = (int)rb.linearVelocity.magnitude + " m/s";
            var velocity = GetVelocity().ToString("F2") + " m/s";
            var grounded = isGrounded ? "Yes" : "No";
            var onSlopeStr = m_onSlope ? "Yes" : "No";
            GUI.Label(new Rect(10, 10, 200, 20), $"State: {stateStr}");
            GUI.Label(new Rect(10, 30, 200, 20), $"Speed: {rbSpeed}");
            GUI.Label(new Rect(10, 50, 200, 20), $"Velocity: {velocity}");
            GUI.Label(new Rect(10, 70, 200, 20), $"Grounded: {grounded}");
            GUI.Label(new Rect(10, 90, 200, 20), $"On Slope: {onSlopeStr}");
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            
            // Draw the ceiling check
            Gizmos.color = Color.red;
            float height = crouchYScale * m_originalYScale;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * (height * 2 + .2f));
            UnityEditor.Handles.Label(transform.position + Vector3.up * (height + .2f), "Ceiling Check");
        }
        
#endif
    }
}*/