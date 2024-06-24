namespace Snowy.FPS
{
    public enum UpdateType
    {
        Update,
        FixedUpdate,
        LateUpdate,
    }
    
    public enum CounterMovementType
    {
        None,
        Friction,
        VelocityLimitHard,
        VelocityLimitForce,
        VelocityLimitLerp,
    }
    
    public enum MovementState
    {
        Idle,
        Walking,
        Sprinting,
        Crouching,
        Sliding,
        Falling
    }
}