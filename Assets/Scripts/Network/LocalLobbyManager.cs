using Network.Client;
using Snowy.Utils;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Network
{
    public class LocalLobbyManager : MonoSingleton<LocalLobbyManager>
    {
        [SerializeField] private NetworkObject clientsManagerPrefab;
        [SerializeField, SceneName] private string gameScene;

        # region Public Functions
        
        /// <summary>
        /// Create a new lobby
        /// </summary>
        /// <param name="maxPlayers"></param>
        /// <returns>bool</returns>
        public bool CreateLobby(int maxPlayers = 8)
        {
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            return NetworkManager.Singleton.StartHost();
        }

        /// <summary>
        /// Join a random lobby
        /// </summary>
        /// <returns>bool</returns>
        public bool JoinRandom()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
            return NetworkManager.Singleton.StartClient();
        }

        /// <summary>
        /// Join a lobby with a specific id
        /// </summary>
        /// <param name="lobbyId"></param>
        public async void JoinLobbyWithId(ulong lobbyId)
        {
            // TODO: Implement this
        }

        # endregion
        
        # region Unity Events
        
        
        private void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
            }
        }
        
        /// <summary>
        /// Make sure we are disconnected from the lobby when the application quits
        /// </summary>
        private void OnApplicationQuit()
        {
            Disconnected();
        }
        
        # endregion
        
        # region Management Functions
        private void Disconnected()
        {
            // Destroy the clients manager
            if (ClientsManager.Instance)
            {
                try
                {
                    ClientsManager.Instance.NetworkObject.Despawn();
                }
                catch
                {
                    Debug.Log("Netcode: Could not Despawn Clients Manager, so we will Destroy it");
                    Destroy(ClientsManager.Instance.gameObject);
                }
            }
            
            if (NetworkManager.Singleton == null) return;
            
            // Remove the callbacks
            if (NetworkManager.Singleton.IsServer)
            {
                NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
            }
            else
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
            }
            
            // We try too shutdown netcode, so we can start a new session later.
            NetworkManager.Singleton.Shutdown();
            Debug.Log("Netcode: Shutdown");
        }
        
        # endregion
        
        # region Netcode NetworkManager Callbacks

        /// <summary>
        /// Netcode: When the host/server is started
        /// </summary>
        private void OnServerStarted()
        {
            Debug.Log("Netcode: Server Started");
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;

            // Check if the clients manager is already spawned
            if (ClientsManager.Instance)
            {
                try
                {
                    ClientsManager.Instance.NetworkObject.Despawn();
                }
                catch
                {
                    Debug.Log("Netcode: Could not Despawn Clients Manager, so we will Destroy it");
                    Destroy(ClientsManager.Instance.gameObject);
                }
            }
            
            // Spawn the clients manager
            NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(clientsManagerPrefab);
            
            // Join the game scene
            NetworkManager.Singleton.SceneManager.LoadScene(gameScene, LoadSceneMode.Single);
        }
        
        /// <summary>
        /// Netcode: When a client connects to the lobby
        /// </summary>
        /// <param name="clientId"></param>
        private void OnClientConnectedCallback(ulong clientId)
        {
            Debug.Log($"Netcode: Client Connected: {clientId}");
        }
        
        /// <summary>
        /// When a client disconnects from the lobby
        /// </summary>
        /// <param name="clientId"></param>
        private void OnClientDisconnectCallback(ulong clientId)
        {
            Debug.Log($"Netcode: Client Disconnected: {clientId}");
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
            if (clientId == 0)
            {
                Disconnected();
            }
        }
        
        #endregion
    }
}