using System;
using System.Collections;
using Network;
using Snowy.Audio;
using Steamworks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

namespace Utils
{
    public class GameStarter : MonoBehaviour
    {
        [SceneName] [SerializeField] private string gameScene;
        [SerializeField] UnityEvent onSteamReady;
        [SerializeField] private UnityEvent onInputReceived;
        private bool isWaitingForInput;
        
        IEnumerator Start()
        {
            yield return new WaitUntil(() => SteamClient.IsLoggedOn);
            Debug.Log($"Steam is ready: {SteamClient.Name}");
            onSteamReady.Invoke();
            isWaitingForInput = true;
            InputSystem.onEvent += OnInputEvent;

            if (SteamLobbyManager.Instance)
            {
                SteamLobbyManager.Instance.OnTryJoinLobby.AddListener(OnTryJoin);
                SteamLobbyManager.Instance.OnJoinFailed.AddListener(OnJoinFailed);
            }
        }

        private void OnTryJoin()
        {
            isWaitingForInput = false;
            InputSystem.onEvent -= OnInputEvent;
        }
        
        private void OnJoinFailed()
        {
            isWaitingForInput = true;
            InputSystem.onEvent += OnInputEvent;
        }

        private void OnDisable()
        {
            InputSystem.onEvent -= OnInputEvent;
            if (SteamLobbyManager.Instance)
            {
                SteamLobbyManager.Instance.OnTryJoinLobby.RemoveListener(OnTryJoin);
                SteamLobbyManager.Instance.OnJoinFailed.RemoveListener(OnJoinFailed);
            }
        }

        private void OnDestroy()
        {
            InputSystem.onEvent -= OnInputEvent;
            if (SteamLobbyManager.Instance)
            {
                SteamLobbyManager.Instance.OnTryJoinLobby.RemoveListener(OnTryJoin);
                SteamLobbyManager.Instance.OnJoinFailed.RemoveListener(OnJoinFailed);
            }
        }

        private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
        {
            # if UNITY_EDITOR
            if (!Application.isPlaying) return;
            # endif
            
            if (!isWaitingForInput) return;
            
            // if mouse return
            if (device is Mouse)
                return;
            
            // Check if pressed any key
            if (eventPtr.IsA<StateEvent>())
            {
                // if is space or enter
                Keyboard.current.spaceKey.ReadValueFromEvent(eventPtr, out var space);
                Keyboard.current.enterKey.ReadValueFromEvent(eventPtr, out var enter);
                
                if (space == 0 && enter == 0) return;
                Debug.Log("Pressed any key");
                isWaitingForInput = false;
                InputSystem.onEvent -= OnInputEvent;
                onInputReceived.Invoke();
                CreateLobby();
            }
        }


        private async void CreateLobby()
        {
            bool isSuccess;
            using (new Loader("", false))
            {
                isSuccess = await SteamLobbyManager.Instance.CreateLobby();
                if (isSuccess)
                {
                    SoundManager.Instance.FadeOutMusic();
                    await LoadingPanel.Instance.LoadBlackScreen(2000);
                    EnterGameScene();
                }
            }
            if(!isSuccess)
            {
                LoadingPanel.Instance.Hide();
                // SnMenuManager.Instance.GoBack();
            }
        }

        /// <summary>
        /// Teleport to the game scene
        /// </summary>
        public void EnterGameScene()
        {
            if (SteamLobbyManager.Instance.CurrentLobby == null) return;
            if (string.IsNullOrEmpty(gameScene)) return;
            
            NetworkManager.Singleton.SceneManager.LoadScene(gameScene, LoadSceneMode.Single);
        }
    }
}