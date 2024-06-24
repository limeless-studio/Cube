using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Snowy.FPS
{
    public class FPSCamera : NetworkBehaviour
    {
        [Title("References")]
        [SerializeField, Tooltip("The object to rotate!")] private Transform camParent;
        [SerializeField, Tooltip("The player main camera")] private Camera cam;

        [Title("Settings")]
        [SerializeField] private Vector3 followOffset;
        [SerializeField] private float sensitivity = 50f;
        [SerializeField, Range(20, 120f)] private float fov = 60f;
        [SerializeField, Range(0, 90f)] private float clampAngle = 70f;
        [SerializeField, Range(0, 2)] private float fovMultiplier = 1;
        [SerializeField, Range(0, 2)] private float runMultiplier = 1.1f;
        
        private Vector2 lookInput;
        private bool isSprinting;
        private static readonly int maxRotCache = 3;
        private float[] rotArrayHor = new float[maxRotCache];
        private float[] rotArrayVert = new float[maxRotCache];
        private int rotCacheIndex;
        
        private float xRot;
        private float yRot;

        private FPSCharacter character;
        private Transform followTarget;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            if (!cam) camParent.GetComponentInChildren<Camera>();
            if (!character) character = GetComponentInParent<FPSCharacter>();
            if (!followTarget) followTarget = character.Movement.Rb.transform;
            
            cam.enabled = IsOwner;
            
            if (IsOwner)
            {
                character.OnInputReceived += HandleInput;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        private void Start()
        {
            if (!cam)
            {
                if (!camParent.TryGetComponent(out cam))
                {
                    cam = GetComponentInChildren<Camera>();
                }
            }
            
            if (!camParent)
            {
                camParent = cam.transform.parent;
            }
            
            if (!character) character = GetComponentInParent<FPSCharacter>();
            
            if (!followTarget) followTarget = character.Movement.Rb.transform;
        }

        private void LateUpdate()
        {
            if (!IsOwner) return;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fov * fovMultiplier * (isSprinting ? runMultiplier : 1), Time.deltaTime * 10f);
            
            // Normal camera Movement
            xRot -= (lookInput.y) * Time.deltaTime;
            xRot = Mathf.Clamp(xRot, -clampAngle, clampAngle);
            
            yRot += lookInput.x * Time.deltaTime;
            
            camParent.localRotation = Quaternion.Euler(xRot, yRot, 0);
            
            // Follow
            Follow();
        }
        
        private void Follow()
        {
            if (!followTarget) return;
            var height = character.GetPlayerHeight() * 0.5f;
            var targetPos = followTarget.position + Vector3.up * height;
            // rotate the follow offset
            var offset = Quaternion.AngleAxis(yRot, Vector3.up) * followOffset;
            transform.position = targetPos + offset;
        }
        
        private void HandleInput(ref PlayerInputs input)
        {
            // Smoothing the input using the average frame solution.
            float x = GetAverageHorizontal(input.lookDir.x);
            float y = GetAverageVertical(input.lookDir.y);
            IncreaseRotCacheIndex();
            
            lookInput = new Vector2(x, y) * sensitivity;
            isSprinting = input.sprint;
        }

        private float GetAverageHorizontal(float h)
        {
            rotArrayHor[rotCacheIndex] = h;
            return rotArrayHor.Average();
        }
        
        private float GetAverageVertical(float v)
        {
            rotArrayVert[rotCacheIndex] = v;
            return rotArrayVert.Average();
        }

        private void IncreaseRotCacheIndex()
        {
            rotCacheIndex++;
            rotCacheIndex %= maxRotCache;
        }
        
        public float GetYRotation()
        {
            return yRot;
        }
        
        public void Spectate()
        {
            cam.enabled = true;
        }
        
        public void UnSpectate()
        {
            cam.enabled = false;
        }
    }
}