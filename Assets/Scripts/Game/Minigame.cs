using System.Collections;
using System.Collections.Generic;
using Network.Client;
using Snowy.NotificationManager;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public class Minigame : NetworkBehaviour
    {
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private MinigameData data;
        
        [Header("Events")]
        public UnityEvent OnMinigameStart;
        public UnityEvent OnMinigameEnd;
        
        public MinigameData Data => data;
        private List<Transform> m_usedSpawnPoints = new List<Transform>();

        public void StartMinigame()
        {
            if (IsHost) StartGame_RPC();
        }
        
        [Rpc(SendTo.Everyone)]
        public void StartGame_RPC()
        {
            StartCoroutine(StartMinigameCo());
        }

        public void EndMinigame()
        {
            Debug.Log($"Ending minigame: {data.title}");
        }

        private IEnumerator StartMinigameCo()
        {
            yield return new WaitForSeconds(1f);
            
            SnNotificationManager.Instance.ShowNotification(NotificationTypeNames.TitleWithContent, data.title,
                data.description, 3f);
            yield return new WaitForSeconds(3f);
            // Countdown
            for (int i = 10; i > 0; i--)
            {
                SnNotificationManager.Instance.ShowNotification(NotificationTypeNames.TitleWithContent, i.ToString(),
                    "Get ready!", 1f);
                yield return new WaitForSeconds(1f);
            }
            
            Debug.Log($"Starting minigame: {data.title}");

            if (IsHost)
            {
                SpawnPlayers();
            }
            
            yield return new WaitForSeconds(1f);
            
            OnMinigameStart.Invoke();
        }

        private Transform GetRandomSpawnPoint()
        {
            if (m_usedSpawnPoints.Count == spawnPoints.Length)
            {
                m_usedSpawnPoints.Clear();
            }

            Transform spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
            while (m_usedSpawnPoints.Contains(spawnPoint))
            {
                spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
            }

            m_usedSpawnPoints.Add(spawnPoint);
            return spawnPoint;
        }

        private void SpawnPlayers()
        {
            if (!IsHost) return;
            foreach (var client in ClientsManager.Instance.GetClients())
            {
                var spawnPoint = GetRandomSpawnPoint();
                client.SpawnAsServer(spawnPoint.position, spawnPoint.rotation);
            }
        }
    }
}