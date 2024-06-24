using Steamworks;
using Unity.Netcode;
using UnityEngine;

namespace Network.Client
{
    public class ClientInfo : NetworkBehaviour
    {
        public string Username { get; set; }
        public ulong SteamId { get; set; }
        public ulong ClientId { get; set; }
        
        public bool IsLocal { get; private set; }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            ClientId = OwnerClientId;
            IsLocal = IsOwner;
            if (IsOwner)
            {
                if (SteamClient.IsValid)
                {
                    SetData_Rpc(SteamClient.Name, SteamClient.SteamId);
                }
                else
                {
                    SetData_Rpc("Local", 0);
                }
            }
            Debug.Log($"Client spawned: {ClientId}");
        }
        
        [Rpc(SendTo.Everyone)]
        public void SetData_Rpc(string username, ulong steamId)
        {
            Username = username;
            SteamId = steamId;
        }
    }
}