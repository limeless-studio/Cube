using Snowy.FPS;
using Unity.Netcode;
using UnityEngine;

namespace Network.Client
{
    [RequireComponent(typeof(ClientInfo))]
    public class Client : NetworkBehaviour
    {
        [SerializeField] private NetworkObject playerPrefab;
        
        private ClientInfo m_clientInfo;
        public ClientInfo ClientInfo => m_clientInfo;
        private NetworkObject m_player;
        private FPSCharacter m_character;

        private void Awake()
        {
            m_clientInfo = GetComponent<ClientInfo>();
            Debug.Assert(m_clientInfo, "ClientInfo component is missing");
            Debug.Log("Client spawned");
            DontDestroyOnLoad(gameObject);
        }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!m_clientInfo) m_clientInfo = GetComponent<ClientInfo>() ?? gameObject.AddComponent<ClientInfo>();
            if (ClientsManager.Instance) ClientsManager.Instance.AddClient(this);
        }
        
        public override void OnDestroy()
        {
            base.OnDestroy();
            if (ClientsManager.Instance) ClientsManager.Instance.RemoveClient(this);
        }
        
        public void SpawnPlayer(Transform spawnPoint)
        {
            if (!playerPrefab) return;
            SpawnPlayer_ServerRPC(spawnPoint.position, spawnPoint.rotation);
        }
        
        [ServerRpc]
        public void SpawnPlayer_ServerRPC(Vector3 position, Quaternion rotation)
        {
            SpawnAsServer(position, rotation);
        }

        public void SpawnAsServer(Vector3 position, Quaternion rotation)
        {
            if (!playerPrefab) return;
            m_player = 
                NetworkManager.Singleton.SpawnManager
                    .InstantiateAndSpawn(playerPrefab, OwnerClientId, true, position: position, rotation: rotation);
        }
        
        public void DespawnPlayer()
        {
            if (!m_player) return;
            m_character = null;
            m_player.Despawn(true);
        }
        
        public void SetCharacter(FPSCharacter character)
        {
            m_character = character;
        }
        
        public FPSCharacter GetCharacter()
        {
            return m_character;
        }
    }
}