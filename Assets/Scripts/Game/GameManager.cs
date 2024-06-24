using System;
using System.Collections;
using Network.Client;
using Snowy.NotificationManager;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace Game
{
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; }
        
        [Title("Game Manager")]
        [SerializeField] private int timeToStart = 5;
        [SerializeField, SceneName] string minigameScene; 
        
        [Space]
        [Title("Game Settings")]
        [SerializeField] int minigamesPerMatch = 3;
        [SerializeField, Disable] Minigame[] minigamePrefabs;
        [SerializeField, Disable] int currentMinigameIndex = 0;
        
        public Minigame CurrentMinigamePrefab => minigamePrefabs[currentMinigameIndex];
        public Minigame CurrentMinigame { get; private set; }
        
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
        
        public void GenerateMinigames()
        {
            minigamePrefabs = new Minigame[minigamesPerMatch];
            int length = Global.Instance.GetMinigameData().Length;
            // Get 3 different minigames
            if (length < minigamesPerMatch)
            {
                minigamePrefabs = Global.Instance.GetMinigameData();
                minigamesPerMatch = length;
                return;
            }
            
            for (int i = 0; i < minigamesPerMatch; i++)
            {
                Minigame minigame = Global.Instance.GetRandomMinigameData();
                while (Array.Exists(minigamePrefabs, data => data == minigame))
                {
                    minigame = Global.Instance.GetRandomMinigameData();
                }
                minigamePrefabs[i] = minigame;
            }
            
            SetMinigames_ClientRpc(Array.ConvertAll(minigamePrefabs, minigame => minigame.Data.id));
        }
        
        [ClientRpc]
        public void SetMinigames_ClientRpc(int[] ids)
        {
            minigamePrefabs = new Minigame[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                minigamePrefabs[i] = Global.Instance.GetMinigameData(ids[i]);
            }
        }

        public void StartStageCountdown()
        {
            if (IsHost)
            {
                StartCountdown_Rpc();
            }
        }
        
        [Rpc(SendTo.Everyone)]
        public void StartCountdown_Rpc()
        {
            StartCoroutine(StartStageCo());
        }
        
        IEnumerator StartStageCo()
        {
            for (int i = timeToStart; i > 0; i--)
            {
                SnNotificationManager.Instance.ShowNotification(NotificationTypeNames.TitleWithContent, i.ToString(),
                    "Teleporting", 1f);
                yield return new WaitForSeconds(1f);
            }
            
            if (IsHost)
            {
                StartStage();
            }
        }

        private void StartStage()
        {
            // 1. Generate minigames
            // 2. Choose a minigame to start
            Debug.Log($"Starting stage: {IsHost}");
            if (!IsHost) return;
            ClientsManager.Instance.DespawnAllPlayers();
            GenerateMinigames();
            TeleportToMinigameScene();
        }
        
        private void TeleportToMinigameScene()
        {
            // Teleport the player to the minigame scene
            NetworkManager.SceneManager.OnSceneEvent += OnSceneEvent;
            NetworkManager.SceneManager.LoadScene(minigameScene, LoadSceneMode.Single);
        }

        private void OnSceneEvent(SceneEvent sceneevent)
        {
            if (sceneevent.SceneEventType == SceneEventType.LoadComplete && sceneevent.SceneName == minigameScene)
            {
                NetworkManager.SceneManager.OnSceneEvent -= OnSceneEvent;
                SpawnMinigame();
            }
        }
        
        private void SpawnMinigame()
        {
            NetworkObject minigamePrefab = CurrentMinigamePrefab.GetComponent<NetworkObject>();
            if (minigamePrefab == null)
            {
                Debug.LogError("Minigame prefab is null");
                return;
            }
            var obj = NetworkManager.SpawnManager.InstantiateAndSpawn(minigamePrefab, 0ul, true);
            if (obj == null)
            {
                Debug.LogError("Failed to spawn minigame");
                return;
            }
            
            // Take off the loading screen
            LoadingPanel.Instance.Hide();
            
            CurrentMinigame = obj.GetComponentInChildren<Minigame>();
            if (CurrentMinigame == null)
            {
                Debug.LogError("Minigame component is missing");
                return;
            }
            
            CurrentMinigame.StartMinigame();
        }
    }
}