using System;
using Snowy.SnInput;
using UnityEngine;

namespace Snowy.FPS
{
    [Serializable] public struct PlayerInputs
    {
        public Vector3 moveDir;
        public Vector2 lookDir;
        public bool sprint;
        public bool jump;
        public bool crouchDown;
        public bool crouch;
        public bool slide;
        public float mouseWheel;
        public ButtonState attack;
        public ButtonState aim;
        public ButtonState interact;
        public ButtonState pickup;
        public ButtonState reload;
        public ButtonState escape;
    }
}