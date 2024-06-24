using Interaction;
using Interface;
using Network.Client;
using Snowy.Actors;
using Snowy.SnInput;
using Unity.Netcode;
using UnityEngine;

namespace Snowy.FPS
{
    public class FPSCharacter : NetworkBehaviour, IDamageable
    {
        public delegate void InputReceived(ref PlayerInputs input);
        public event InputReceived OnInputReceived;
        public event InputReceived OnUpdate;
        
        [Title("Player Settings")]
        [SerializeField] private float playerHeight = 2f;
        [SerializeField, Disable] private float currentHeight;
        
        [Title("Actor")]
        [SerializeField] NetworkVariable<float> health = new(100f);
        [SerializeField] public float maxHealth = 100f;
        [SerializeField] public bool startWithMaxHealth = true;
        [SerializeField] public float startingHealth = 100f;
        
        public float Health => health.Value;
        public bool IsDead => health.Value <= 0;
        
        private PlayerInputs m_input;

        private FPSMovement m_movement;
        private FPSCamera m_camera;
        private InterfaceManager m_interfaceManager;
        private Interactor m_interactor;
        
        public FPSMovement Movement
        {
            get
            {
                if (!m_movement)
                    m_movement = GetComponentInChildren<FPSMovement>();
                return m_movement;
            }
        }
        
        public FPSCamera Camera
        {
            get
            {
                if (!m_camera)
                    m_camera = GetComponentInChildren<FPSCamera>();
                return m_camera;
            }
        }
        
        public InterfaceManager InterfaceManager
        {
            get
            {
                if (!m_interfaceManager)
                    m_interfaceManager = GetComponentInChildren<InterfaceManager>();
                return m_interfaceManager;
            }
        }
        
        public Interactor Interactor
        {
            get
            {
                if (!m_interactor)
                    m_interactor = GetComponentInChildren<Interactor>();
                return m_interactor;
            }
        }
        
        private bool canMove = true;
        private bool canLook = true;
        
        # region Netcode Functions

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            health.Value = startWithMaxHealth ? maxHealth : startingHealth;

            ClientsManager.Instance.GetClient(OwnerClientId).SetCharacter(this);
            
            if (Interactor)
            {
                Interactor.OnSpawn(IsOwner);
            }
        }

        #endregion
        
        # region Unity Functions
        
        protected virtual void Awake()
        {
            SetPlayerHeight(playerHeight);
        }

        protected virtual void Update()
        {
            if (!IsOwner) return;
            GetInput();
            OnUpdate?.Invoke(ref m_input);
        }
        
        #endregion
        
        private void GetInput()
        {
            if (!IsOwner) return;
            if (InputManager.Instance)
            {
                m_input.moveDir = canMove ? InputManager.Instance.MoveInput : Vector2.zero;
                m_input.lookDir = canLook ? InputManager.Instance.LookInput : Vector2.zero;

                m_input.jump = InputManager.Instance.IsJump;
                m_input.sprint = InputManager.Instance.IsSprint;
                m_input.crouch = InputManager.Instance.IsCrouch;
                m_input.slide = InputManager.Instance.IsSlide;
                m_input.pickup = InputManager.Instance.PickState;
                m_input.interact = InputManager.Instance.InteractState;
                
                m_input.attack = InputManager.Instance.AttackState;
                m_input.aim = InputManager.Instance.AimState;
                m_input.reload = InputManager.Instance.ReloadState;
                
                m_input.mouseWheel = InputManager.Instance.MouseWheel;
                m_input.escape = InputManager.Instance.EscapeState;
            }
            
            OnInputReceived?.Invoke(ref m_input);
        }
        
        #region Settings
        
        public void SetPlayerHeight(float height)
        {
            currentHeight = height;
            // Set the player height to the new height
            Movement.transform.localScale = new Vector3(1, height / 2, 1);
        }
        
        public float GetPlayerHeight()
        {
            return currentHeight;
        }
        
        public void ResetHeight()
        {
            SetPlayerHeight(playerHeight);
        }
        
        public void SetCanMove(bool value)
        {
            canMove = value;
        }
        
        public void SetCanLook(bool value, bool freeCursor = false)
        {
            canLook = value;
            if (freeCursor)
            {
                FreeCursor();
            }
        }
        
        public void FreeCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        public void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        #endregion
        
        #region Actor Functions
        
        public void SendTakeDamage(float damage)
        {
            TakeDamage_ServerRpc(damage);
        }
        
        [ServerRpc]
        public void TakeDamage_ServerRpc(float damage)
        {
            TakeDamage(damage);
        }
        
        public void TakeDamage(float damage)
        {
            if (!IsHost) return;
            health.Value -= damage;
            if (health.Value <= 0)
            {
                Die();
            }
        }

        public void TakeDamage(float damage, DamageCause cause)
        {
            if (!IsHost) return;
            health.Value -= damage;
            if (health.Value <= 0)
            {
                Die();
            }
        }
        
        private void Die()
        {
            if (!IsHost) return;
            health.Value = 0;
            Die_Rpc();
        }

        [Rpc(SendTo.Everyone)]
        public void Die_Rpc()
        {
            // Despawn
            gameObject.SetActive(false);
        }
        
        #endregion
    }
}