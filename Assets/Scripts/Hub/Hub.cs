using System.Collections;
using Game;
using Network.Client;
using Unity.Netcode;
using UnityEngine;
using Utils;

namespace Hub
{
    public class Hub : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] private GameManager gameManagerPrefab;
        [SerializeField] private GameObject spawnPoint;
        
        private void Start()
        {
            if (LoadingPanel.Instance)
                LoadingPanel.Instance.Hide();
            
            if (ClientsManager.Instance)
                ClientsManager.Instance.GetLocalClient()?.SpawnPlayer(spawnPoint.transform);

            if (NetworkManager.Singleton.IsHost)
            {
                // Spawn the game manager
                var manager = Instantiate(gameManagerPrefab);
                manager.NetworkObject.SpawnWithOwnership(0ul);
            }
        }


        public void StartStage()
        {
            if (GameManager.Instance)
                GameManager.Instance.StartStageCountdown();
        }
    }
}