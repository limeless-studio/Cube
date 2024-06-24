// Disable warning for unused variables & unused events
#pragma warning disable 0414
#pragma warning disable 67

using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Snowy.SnInput
{
    public enum ButtonState
    {
        Pressed,
        Released,
        Held,
        None
    }
    
    public class InputManager : MonoBehaviour 
    {
        public static InputManager Instance { get; private set; }
        
        [SerializeField] private PlayerInput playerInput;
        public bool isInputEnabled = true;

        #region Move & Look
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public float Horizontal => MoveInput.x;
        public float Vertical => MoveInput.y;
        public float MouseWheel { get; private set; }

        #endregion
        
        #region Private Action bools
        private bool m_jump;
        private bool m_attack;
        private bool m_aim;
        private bool m_sprint;
        private bool m_crouch;
        private bool m_slide;
        private bool m_pick;
        private bool m_interact;
        private bool m_reload;
        private bool m_escape;
        #endregion
        
        #region Public Action States & bools
        public bool IsJump => m_jump;
        public bool IsAttack => m_attack;
        public bool IsAim => m_aim;
        public bool IsSprint => m_sprint;
        public bool IsCrouch => m_crouch;
        public bool IsSlide => m_slide;
        public bool IsReadying => m_pick;
        public bool IsInteracting => m_interact;
        public bool IsReloading => m_reload;
        public bool IsEscaping { get; private set; }
        
        public ButtonState JumpState { get; private set; }
        public ButtonState AttackState { get; private set; }
        public ButtonState AimState { get; private set; }
        public ButtonState SprintState { get; private set; }
        public ButtonState CrouchState { get; private set; }
        public ButtonState SlideState { get; private set; }
        public ButtonState PickState { get; private set; }
        public ButtonState InteractState { get; private set; }
        public ButtonState ReloadState { get; private set; }
        public ButtonState EscapeState { get; private set; }
        
        public event Action<ButtonState> OnJump;
        public event Action<ButtonState> OnAttack;
        public event Action<ButtonState> OnAim;
        public event Action<ButtonState> OnRun;
        public event Action<ButtonState> OnCrouch;
        public event Action<ButtonState> OnSlide;
        public event Action<ButtonState> OnPick;
        public event Action<ButtonState> OnInteract;
        public event Action<ButtonState> OnReload;
        public event Action<ButtonState> OnEscape;
        #endregion
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (playerInput == null)
            {
                playerInput = GetComponent<PlayerInput>();
            }
            
            if (playerInput == null)
            {
                Debug.LogError("PlayerInput component not found on GameObject");
            }
            
            playerInput.onActionTriggered += OnActionTriggered;
        }
        
        private void Update()
        {
            JumpState = ButtonUpdate(m_jump, JumpState, OnJump);
            AttackState = ButtonUpdate(m_attack, AttackState, OnAttack);
            AimState = ButtonUpdate(m_aim, AimState, OnAim);
            SprintState = ButtonUpdate(m_sprint, SprintState, OnRun);
            CrouchState = ButtonUpdate(m_crouch, CrouchState, OnCrouch);
            SlideState = ButtonUpdate(m_slide, SlideState, OnSlide);
            PickState = ButtonUpdate(m_pick, PickState, OnPick);
            InteractState = ButtonUpdate(m_interact, InteractState, OnInteract);
            ReloadState = ButtonUpdate(m_reload, ReloadState, OnReload);
            EscapeState = ButtonUpdate(m_escape, EscapeState, OnEscape);
        }

        private void OnActionTriggered(InputAction.CallbackContext context)
        {
            if (!isInputEnabled)
            {
                return;
            }

            switch (context.action.name)
            {
                case "Move":
                    MoveInput = context.ReadValue<Vector2>();
                    break;
                case "Look":
                    LookInput = context.ReadValue<Vector2>();
                    break;
                case "Jump":
                    m_jump = context.ReadValueAsButton();
                    break;
                case "Attack":
                    m_attack = context.ReadValueAsButton();
                    break;
                case "Aim":
                    m_aim = context.ReadValueAsButton();
                    break;
                case "Sprint":
                    m_sprint = context.ReadValueAsButton();
                    break;
                case "Crouch":
                    m_crouch = context.ReadValueAsButton();
                    break;
                case "Slide":
                    m_slide = context.ReadValueAsButton();
                    break;
                case "Pickup":
                    m_pick = context.ReadValueAsButton();
                    break;
                case "Interact":
                    m_interact = context.ReadValueAsButton();
                    break;
                case "Reload":
                    m_reload = context.ReadValueAsButton();
                    break;
                case "MouseWheel":
                    MouseWheel = context.ReadValue<float>();
                    break;
                case "Escape":
                    m_escape = context.ReadValueAsButton();
                    break;
            }
        }

        private ButtonState ButtonUpdate(bool button, ButtonState stateState, Action<ButtonState> action = null)
        {
            if (button)
            {
                if (stateState == ButtonState.None || stateState == ButtonState.Released)
                {
                    stateState = ButtonState.Pressed;
                }
                else
                {
                    stateState = ButtonState.Held;
                }
                
                action?.Invoke(stateState);
            }
            else
            {
                if (stateState == ButtonState.Pressed || stateState == ButtonState.Held)
                {
                    stateState = ButtonState.Released;
                    
                    action?.Invoke(stateState);
                }
                else
                {
                    stateState = ButtonState.None;
                }
            }
            
            return stateState;
        }
        
        public string GetCurrentControlScheme()
        {
            return playerInput.currentControlScheme;
        }
        
        public void ListenToSchemeChange(Action<string> action)
        {
            playerInput.onControlsChanged += context => action?.Invoke(context.currentControlScheme);
        }
    }
}