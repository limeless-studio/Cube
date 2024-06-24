using Unity.Netcode;
using UnityEngine;

namespace Minigames.TntRun
{
    public class TileManager : NetworkBehaviour
    {
        public static TileManager Instance { get; private set; }
        [SerializeField] TntTile[] tiles;
        private bool started;
        
        private void Awake()
        {
            Instance = this;
            started = false;
        }
        
        public void StartGame()
        {
            started = true;
        }
        
        public void EndGame()
        {
            started = false;
        }
        
        public void TileTriggered(TntTile tile)
        {
            if (!IsHost) return;
            if (!started) return;
            
            var index = GetTileIndex(tile);
            if (index == -1)
            {
                Debug.LogError("Tile not found");
                return;
            }
            
            TriggerTile_ClientRpc(index);
            tile.StartCoroutine(tile.ShakeAndFall());
        }
        
        [ClientRpc]
        public void TriggerTile_ClientRpc(int tileIndex)
        {
            var tile = tiles[tileIndex];
            tile.StartCoroutine(tile.ShakeAndFall());
        }
        
        private int GetTileIndex(TntTile tile)
        {
            for (var i = 0; i < tiles.Length; i++)
            {
                if (tiles[i] == tile)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}