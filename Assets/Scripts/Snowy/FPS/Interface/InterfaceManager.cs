using System;
using Snowy.FPS;
using Snowy.SnInput;
using Snowy.Utils;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Interface
{
    public class InterfaceManager : NetworkBehaviour
    {
        [SerializeField] private GameObject escapeMenu;
        [SerializeField] private GameObject canvas;
        
        public FPSCharacter character;
        private event Action OnUpdateAction;
        
        bool isPaused;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsOwner)
            {
                if (canvas) canvas.SetActive(true);
                character = GetComponentInParent<FPSCharacter>();
                if (character)
                {
                    character.OnUpdate += OnUpdate;
                }
                Resume();
            } else
            {
                if (canvas) canvas.SetActive(false);
            }
        }
        
        private void OnUpdate(ref PlayerInputs input)
        {
            OnUpdateAction?.Invoke();

            if (input.escape != ButtonState.None)
            {
                if (input.escape == ButtonState.Pressed)
                {
                    if (isPaused) Resume();
                    else Pause();
                }
            }
        }

        public void RegisterElement(Element element)
        {
            OnUpdateAction += element.Run;
        }

        public void UnRegisterElement(Element element)
        {
            OnUpdateAction -= element.Run;
        }

        public void Pause()
        {
            isPaused = true;
            character.SetCanMove(false);
            character.SetCanLook(false, true);
            if (escapeMenu) escapeMenu.SetActive(true);
        }
        
        public void Resume()
        {
            isPaused = false;
            character.SetCanMove(true);
            character.SetCanLook(true);
            character.LockCursor();
            if (escapeMenu) escapeMenu.SetActive(false);
        }
    }
}