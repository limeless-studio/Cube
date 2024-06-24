using System;
using System.Collections;
using Network.Client;
using Unity.Netcode;
using UnityEngine;

namespace Network
{
    public class SceneWatcher : MonoBehaviour
    {
        public static SceneWatcher Instance { get; private set; }
        
        [SerializeField, SceneName] string hubScene;
        
        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        IEnumerator Start()
        {
            yield return new WaitUntil(() => NetworkManager.Singleton != null);
            if (!NetworkManager.Singleton.NetworkConfig.EnableSceneManagement) yield break;
            yield return new WaitUntil(() => NetworkManager.Singleton.SceneManager != null);
            NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneEvent;
        }

        private void OnDisable()
        {
            if (NetworkManager.Singleton) NetworkManager.Singleton.SceneManager.OnSceneEvent -= OnSceneEvent;
        }
        
        private void OnSceneEvent(SceneEvent sceneEvent)
        {
            /*Debug.Log($"Scene event: {sceneEvent.SceneEventType}");
            switch (sceneEvent.SceneEventType)
            {
                case SceneEventType.LoadComplete:
                    if (sceneEvent.SceneName == hubScene)
                    {
                        Debug.Log("Hub scene loaded");
                        //TODO: Spawn hub player
                        if (ClientsManager.Instance)
                            ClientsManager.Instance.GetLocalClient()?.SpawnPlayer();
                    }
                    break;
            }*/
        }
    }
}