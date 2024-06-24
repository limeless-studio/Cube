using System.Linq;
using System.Threading.Tasks;
using Netcode.Transports.Facepunch;
using Network.Client;
using Snowy.NotificationManager;
using Snowy.NotificationManager.Extra;
using Snowy.Utils;
using Steamworks;
using Steamworks.Data;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Utils;

namespace Network
{
    public class SteamLobbyManager : MonoSingleton<SteamLobbyManager>
    {
        #region Constants

        public const string KEY = "snowy-1.0.0";
        public const string VALUE = "snowy-1.0.0";

        #endregion
        
        [SerializeField] private NetworkObject clientsManagerPrefab;
        public UnityEvent OnTryJoinLobby;
        public UnityEvent OnJoinFailed;
        private FacepunchTransport m_transport;
        
        /// <summary>
        /// The current lobby
        /// </summary>
        public Lobby? CurrentLobby { get; private set; }
        
        # region Public Functions
        
        /// <summary>
        /// Create a new lobby
        /// </summary>
        /// <param name="maxPlayers"></param>
        /// <returns>bool</returns>
        public async Task<bool> CreateLobby(int maxPlayers = 8)
        {
            // Disconnected from the current lobby
            await Disconnected();
            OnTryJoinLobby.Invoke();
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            CurrentLobby = await SteamMatchmaking.CreateLobbyAsync(maxPlayers);
            if (CurrentLobby.HasValue)
            {
                Debug.Log($"Created Lobby: {CurrentLobby.Value.Id}");
                return NetworkManager.Singleton.StartHost();
            }
            Debug.LogError("Failed to Create Lobby");
            OnJoinFailed.Invoke();
            return false;
        }

        /// <summary>
        /// Join a random lobby
        /// </summary>
        /// <returns>bool</returns>
        public async Task<bool> JoinRandom()
        {
            OnTryJoinLobby.Invoke();
            Lobby[] lobbies = await SteamMatchmaking.LobbyList.WithSlotsAvailable(1).RequestAsync();
            if (lobbies.Length == 0)
            {
                Debug.LogError("No Lobbies Found");
                return false;
            }
            // Filter the lobbies
            lobbies = lobbies.Where(x => x.GetData(KEY) == VALUE).ToArray();
            // Get a random lobby
            int randomIndex = Random.Range(0, lobbies.Length);
            var lobby = lobbies[randomIndex];
            bool isSuccess = await JoinLobby(lobby);
            
            if (!isSuccess) OnJoinFailed.Invoke();
            return isSuccess;
        }

        /// <summary>
        /// Join a lobby with a specific id
        /// </summary>
        /// <param name="lobbyId"></param>
        public async Task<bool> JoinLobbyWithId(ulong lobbyId)
        {
            // Disconnected from the current lobby
            await Disconnected();
            OnTryJoinLobby.Invoke();
            
            Lobby? lobby = await SteamMatchmaking.JoinLobbyAsync(lobbyId);
            if (lobby.HasValue)
            {
                CurrentLobby = lobby;
            }
            
            if (!lobby.HasValue) OnJoinFailed.Invoke();
            return lobby.HasValue;
        }

        /// <summary>
        /// Join a specific lobby
        /// </summary>
        /// <param name="lobby"></param>
        /// <returns></returns>
        public async Task<bool> JoinLobby(Lobby lobby)
        {
            // Disconnected from the current lobby
            await Disconnected();
            OnTryJoinLobby.Invoke();
            
            RoomEnter roomEnter = await lobby.Join();
            if (roomEnter == RoomEnter.Success)
            {
                CurrentLobby = lobby;
                return true;
            }
            OnJoinFailed.Invoke();
            return false;
        }

        # endregion
        
        # region Unity Events
        
        private void Start()
        {
            m_transport = GetComponent<FacepunchTransport>();
            SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
            SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
            SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
            SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;
            SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreated;
            SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
        }
        
        private void OnDestroy()
        {
            SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
            SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
            SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
            SteamMatchmaking.OnLobbyInvite -= OnLobbyInvite;
            SteamMatchmaking.OnLobbyGameCreated -= OnLobbyGameCreated;
            SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;

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
            _ = Disconnected(false);
        }
        
        # endregion
        
        # region Management Functions
        
        private void StartClient(SteamId steamId)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
            m_transport.targetSteamId = steamId;
            if (NetworkManager.Singleton.StartClient())
            {
                Debug.Log("Netcode: Client Started");
            }
            else
            {
                Debug.LogError("Netcode: Client Failed to Start");
            }
        }
        
        private async Task Disconnected(bool loadMenu = true)
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
            
            // First we leave the steam lobby
            CurrentLobby?.Leave();
            if (CurrentLobby.HasValue)
            {
                Debug.Log($"Left Lobby: {CurrentLobby.Value.Id}");
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
            
            // Load the main menu
            if (SceneManager.GetActiveScene().buildIndex != 0 && loadMenu)
            {
                await SceneManager.LoadSceneAsync(0);
            }
        }

        private async void OnInviteAccepted(SnSubmitNotification notification, Lobby lobby)
        {
            notification.OnSubmit.RemoveAllListeners();
            
            bool isSuccess;
            using (new Loader("", false))
            {
                await LoadingPanel.Instance.LoadBlackScreen(500);
                isSuccess = await JoinLobby(lobby);
            }
            if(!isSuccess) LoadingPanel.Instance.Hide();
            
        }
        
        private void OnInviteDeclined(SnSubmitNotification notification, Lobby lobby)
        {
            notification.OnCancel.RemoveAllListeners(); 
        }

        # endregion
        
        # region Netcode NetworkManager Callbacks

        /// <summary>
        /// Netcode: When the host/server is started
        /// </summary>
        private void OnServerStarted()
        {
            Debug.Log("Netcode: Server Started");
            NetworkManager.Singleton.OnServerStopped += OnServerStopped;
            
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
        }
        
        /// <summary>
        /// Netcode: When a client connects to the lobby
        /// </summary>
        /// <param name="clientId"></param>
        private void OnClientConnectedCallback(ulong clientId)
        {
            Debug.Log($"Netcode: Client Connected: {clientId}");
            NetworkManager.Singleton.OnClientStopped += OnClientStopped;
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
                _ = Disconnected();
            }
        }
        
        private void OnServerStopped(bool add)
        {
            Debug.Log("Netcode: Server Stopped");
            NetworkManager.Singleton.OnServerStopped -= OnServerStopped;
            _ = Disconnected();
        }
        
        private void OnClientStopped(bool add)
        {
            Debug.Log("Netcode: Client Stopped");
            NetworkManager.Singleton.OnClientStopped -= OnClientStopped;
            _ = Disconnected();
        }
        
        #endregion
        
        # region SteamMatchmaking Callbacks
        
        /// <summary>
        /// When the local player creates a lobby
        /// </summary>
        /// <param name="result"></param>
        /// <param name="lobby"></param>
        private void OnLobbyCreated(Result result, Lobby lobby)
        {
            Debug.Log($"OnLobbyCreated: {lobby.Id}");
            if (result != Result.OK)
            {
                Debug.LogError("Failed to Create Lobby");
                return;
            }
            CurrentLobby = lobby;
            
            lobby.SetPublic();
            lobby.SetJoinable(true);
            lobby.SetGameServer(lobby.Owner.Id);
            lobby.SetData(KEY, VALUE);
        }
        
        /// <summary>
        /// When the local player joins the lobby
        /// </summary>
        /// <param name="lobby"></param>
        private void OnLobbyEntered(Lobby lobby)
        {
            Debug.Log($"OnLobbyEntered: {lobby.Id}");
            CurrentLobby = lobby;
            
            // If not server then start client;
            if (!lobby.Owner.IsMe)
                StartClient(lobby.Owner.Id);
        }
        
        /// <summary>
        /// When a player leaves the lobby
        /// </summary>
        /// <param name="lobby"></param>
        /// <param name="friend"></param>
        private async void OnLobbyMemberLeave(Lobby lobby, Friend friend)
        {
            Debug.Log($"OnLobbyMemberLeave: {lobby.Id}");
            // If the owner leaves the lobby, then we should disconnect from the lobby
            Debug.Log($"Owner: {lobby.Owner.Name} Friend: {friend.Name}");
            Debug.Log($"Owner: {lobby.Owner.Id} Friend: {friend.Id}");
            if (lobby.Owner.Id == friend.Id)
            {
                try
                {
                    await Disconnected();
                }
                catch
                {
                    Debug.LogError("Something went wrong while disconnecting from the lobby");
                    // Load the main menu
                    await SceneManager.LoadSceneAsync(0);
                }
            }
        }
        
        /// <summary>
        /// When a player joins the lobby
        /// </summary>
        /// <param name="lobby"></param>
        /// <param name="friend"></param>
        private void OnLobbyMemberJoined(Lobby lobby, Friend friend)
        {
            Debug.Log($"OnLobbyMemberJoined: {lobby.Id}");
        }
        
        /// <summary>
        /// When a player invites you to a lobby
        /// </summary>
        /// <param name="friend"></param>
        /// <param name="lobby"></param>
        private void OnLobbyInvite(Friend friend, Lobby lobby)
        {
            Debug.Log($"OnLobbyInvite: {lobby.Id}");
            var notification =
                SnNotificationManager.Instance.ShowNotification(NotificationTypeNames.SideSubmit,
                    $"{friend.Name} Invited you!", duration: 10) as SnSubmitNotification;
            notification?.OnSubmit.AddListener(() => OnInviteAccepted(notification, lobby));
            notification?.OnCancel.AddListener(() => OnInviteDeclined(notification, lobby));

            // TODO: Show a dialog to accept or decline the invite
        }
        
        /// <summary>
        /// When the steam lobby is connected with the server
        /// </summary>
        /// <param name="lobby"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="steamId"></param>
        private void OnLobbyGameCreated(Lobby lobby, uint ip, ushort port, SteamId steamId)
        {
            Debug.Log($"OnLobbyGameCreated: {lobby.Id}");
        }
        
        /// <summary>
        /// When an uninvited player tries to join the lobby
        /// </summary>
        /// <param name="lobby"></param>
        /// <param name="steamId"></param>
        private async void OnGameLobbyJoinRequested(Lobby lobby, SteamId steamId)
        {
            // TODO: Show a dialog to accept or decline the Request
            Debug.Log($"OnGameLobbyJoinRequested: {steamId}");
        }
        
        # endregion
    }
}