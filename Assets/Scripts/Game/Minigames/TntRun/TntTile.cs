using System;
using System.Collections;
using UnityEngine;

namespace Minigames.TntRun
{
    public class TntTile : MonoBehaviour
    {
        [SerializeField, TagSelector] private string playerTag;
        [SerializeField] private GameObject tile;
        [SerializeField] private bool didFall;
        [SerializeField] private float shakeDuration;
        [SerializeField] private float shakeMagnitude;

        private void Start()
        {
            if (tile == null)
            {
                tile = gameObject;
            }
        }

        private void OnTriggerEnter(Collider other) 
        {
            if (other.CompareTag(playerTag))
            {
                if (didFall) return;
                TileManager.Instance.TileTriggered(this);
            }
        }

        public IEnumerator ShakeAndFall()
        {
            if (didFall) yield break;
            didFall = true;
            // Shake the tile
            var originalPos = tile.transform.position;
            var elapsed = 0f;
            while (elapsed < shakeDuration)
            {
                var x = originalPos.x + UnityEngine.Random.Range(-1f, 1f) * shakeMagnitude;
                var z = originalPos.z + UnityEngine.Random.Range(-1f, 1f) * shakeMagnitude;
                tile.transform.position = new Vector3(x, originalPos.y, z);
                elapsed += Time.deltaTime;
                yield return null;
            }
            tile.transform.position = originalPos;
            
            // fall
            StartFall();
        }
        
        public void StartFall()
        {
            var rb = tile.AddComponent<Rigidbody>();
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            Destroy(tile, 5f);
            Destroy(gameObject, 5f);
        }
    }
}