using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Network.Client
{
    public class ClientsManager : NetworkBehaviour
    {
        public static ClientsManager Instance { get; private set; }
        private Dictionary<ulong, Client> m_clients = new Dictionary<ulong, Client>();

        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Register all clients missing from the dictionary
            foreach (var client in FindObjectsByType<Client>(FindObjectsSortMode.None))
            {
                m_clients.TryAdd(client.OwnerClientId, client);
            }
        }
        
        public void AddClient(Client client)
        {
            m_clients[client.OwnerClientId] = client;
        }
        
        public void RemoveClient(Client client)
        {
            m_clients.Remove(client.OwnerClientId);
        }
        
        public Client GetClient(ulong clientId)
        {
            return m_clients.GetValueOrDefault(clientId);
        }
        
        public Client GetLocalClient()
        {
            return GetClient(NetworkManager.Singleton.LocalClientId);
        }

        public void Spectate()
        {
            // Get a random client to spectate other than the local client
            var clients = m_clients.Values;
            var localClient = GetLocalClient();
            var client = clients.FirstOrDefault(c => c != localClient && !c.GetCharacter().IsDead);
            if (client == null)
            {
                Debug.Log("No clients to spectate");
                return;
            }
            
            Debug.Log($"Spectating {client.OwnerClientId}");
            localClient.GetCharacter().Camera.Spectate();
        }
        
        public IEnumerable<Client> GetClients()
        {
            return m_clients.Values;
        }
        
        public void DespawnAllPlayers()
        {
            foreach (var client in m_clients.Values)
            {
                client.DespawnPlayer();
            }
        }
    }
}