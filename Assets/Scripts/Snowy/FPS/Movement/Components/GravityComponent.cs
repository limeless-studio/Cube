using UnityEngine;

namespace Snowy.FPS
{
    public class GravityComponent : MovementComponent
    {
        [SerializeField] private float gravity = 10f;
        [SerializeField] private bool onlyWhenNotGrounded = true;
        
        public override void OnMovementUpdate()
        {
            if (!IsGrounded() || !onlyWhenNotGrounded)
                AddForce(Vector3.down * gravity, ForceMode.Acceleration);
        }
    }
}