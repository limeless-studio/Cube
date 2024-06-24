using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Utils
{
    // Loading Disposal
    public class Loader : IDisposable
    {
        public bool HideOnDispose = true;
        
        public Loader(string message = "Loading...", bool hideOnDispose = true)
        {
            HideOnDispose = hideOnDispose;
            if (LoadingPanel.Instance)
            {
                LoadingPanel.Instance.Show(message);
            }
        }

        public void Dispose()
        {
            if (LoadingPanel.Instance && HideOnDispose)
            {
                LoadingPanel.Instance.Hide();
            }
        }
        
        public void SetMessage(string message)
        {
            if (LoadingPanel.Instance)
            {
                LoadingPanel.Instance.Show(message);
            }
        }
    }
    
    public class LoadingPanel : MonoBehaviour
    {
        public static LoadingPanel Instance { get; private set; }
        
        [Header("Loading Panel")]
        [SerializeField] CanvasGroup loadingPanel;
        [SerializeField] TMP_Text loadingText;
        [SerializeField] Image loadingImage;
        [SerializeField] float fadeDuration = 0.5f;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            
            DontDestroyOnLoad(gameObject);
            
            if (loadingImage) loadingImage.CrossFadeAlpha(0, 0, true);
            Hide();
        }
        
        public void Show(string message = "Loading...")
        {
            if (loadingText) loadingText.text = message;
            loadingPanel.gameObject.SetActive(true);
            StartCoroutine(Fade(1));
        }

        IEnumerator Fade(float target, bool state = true)
        {
            float start = loadingPanel.alpha;
            float t = 0;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                loadingPanel.alpha = Mathf.Lerp(start, target, t / fadeDuration);
                yield return null;
            }
            loadingPanel.alpha = target;
            if (!state) loadingPanel.gameObject.SetActive(false);
        }

        public void FadeIn()
        {
            if (loadingImage) loadingImage.CrossFadeAlpha(1, fadeDuration, true);
        }
        
        public void FadeOut()
        {
            if (loadingImage)loadingImage.CrossFadeAlpha(0, fadeDuration, true);
        }
        
        public void Hide()
        {
            FadeOut();
            StartCoroutine(Fade(0, false));
        }

        async public void LoadSceneAsync(string sceneName, int delay = 5000)
        {
            // Load async
            using (new Loader(""))
            {
                await Task.Delay(delay);
                // Fade in
                FadeIn();
                
                // Load scene
                var asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
                if (asyncOperation == null)
                {
                    return;
                }
                asyncOperation.allowSceneActivation = false;
                
                // Wait for scene to load
                while (asyncOperation.progress < 0.9f)
                {
                    await Task.Yield();
                }
                
                // Wait half a second
                await Task.Delay(500);
                // Activate scene
                asyncOperation.allowSceneActivation = true;
                
                // wait half a second then Fade out 
                await Task.Delay(1000);
                FadeOut();
                
                // Wait half a second
                await Task.Delay(500);
            }
        }

        public async Task LoadBlackScreen(int delay = 5000)
        {
            await Task.Delay(delay);
            // Fade in
            FadeIn();
            // Wait for the duration
            await Task.Delay((int)fadeDuration * 1000 + 1000);
        }
    }
}