using UnityEngine;

namespace Snowy.FPS
{
    public interface IMovement
    {
        void CheckGrounded();

        Vector3 GetVelocity() => Vector3.zero;

        public bool IsGrounded() => false;
    }
}