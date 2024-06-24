using Network;
using Snowy.Audio;
using Snowy.Menu;
using UnityEngine;
using Utils;

namespace UI
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float cameraDuration = 1f;
        [SerializeField, SceneName] private string gameScene;
        [SerializeField] private SnMenu loadingMenu;
        [SerializeField] private AudioClip music;
        
        Vector3 cameraStartPosition;
        
        private void Start()
        {
            cameraStartPosition = cameraTransform.position;
            SoundManager.Instance.PlayMusic(music);
        }
        
        public void StartGame()
        {
            loadingMenu.OpenMenu();
            SoundManager.Instance.FadeOutMusic();
            LoadingPanel.Instance.LoadSceneAsync(gameScene, 4);
        }
        
        public async void HostLobby()
        {
            loadingMenu.OpenMenu();
            bool isSuccess;
            using (new Loader("", false))
            {
                isSuccess = await SteamLobbyManager.Instance.CreateLobby();
                if (isSuccess)
                {
                    SoundManager.Instance.FadeOutMusic();
                    await LoadingPanel.Instance.LoadBlackScreen(2000);
                }
            }
            if(!isSuccess)
            {
                LoadingPanel.Instance.Hide();
                SnMenuManager.Instance.GoBack();
            }
        }

        public async void QuickJoin()
        {
            loadingMenu.OpenMenu();
            bool isSuccess;
            using (new Loader("", false))
            {
                isSuccess = true;//await CustomNetworkManager.Instance.CreateOrJoinLobby();
                if (isSuccess)
                {
                    SoundManager.Instance.FadeOutMusic();
                    await LoadingPanel.Instance.LoadBlackScreen(2000);
                    // CustomNetworkManager.Instance.StartGame();
                }
            }
            if(!isSuccess)
            {
                LoadingPanel.Instance.Hide();
                SnMenuManager.Instance.GoBack();
            }
        }

        public void QuitGame()
        {
            # if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            # else
            Application.Quit();
            # endif
        }
    }
}