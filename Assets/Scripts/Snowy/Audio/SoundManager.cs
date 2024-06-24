using System.Collections;
using Snowy.Menu;
using UnityEngine;

namespace Snowy.Audio
{
    public class SoundManager : MonoBehaviour
    {
        private static SoundManager m_soundManager;
        public static SoundManager Instance {
            get
            {
                if (m_soundManager == null)
                {
                    m_soundManager = FindFirstObjectByType<SoundManager>();
                }
                
                return m_soundManager;
            }
            private set => m_soundManager = value;
        }
        
        public MenuClipsTheme menuClipsTheme;
        [SerializeField] private AudioSource uiAudioSource;
        [SerializeField] private AudioSource musicAudioSource;
        [SerializeField] private AudioSource defaultAudioSource;
        
        private void Awake()
        {
            if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (!uiAudioSource)
            {
                uiAudioSource = gameObject.AddComponent<AudioSource>();
            }
            
            if (!musicAudioSource)
            {
                musicAudioSource = gameObject.AddComponent<AudioSource>();
            }
            
            if (!defaultAudioSource)
            {
                defaultAudioSource = gameObject.AddComponent<AudioSource>();
            }
            
            uiAudioSource.loop = false;
            musicAudioSource.loop = true;
            defaultAudioSource.loop = false;
        }
        
        public void PlaySound(AudioClip clip, AudioSource source = null)
        {
            if (!clip) return;
            source ??= defaultAudioSource;
            source.PlayOneShot(clip);
        }
        
        public void PlayUISound(AudioClip clip)
        {
            if (!clip || !uiAudioSource) return;
            uiAudioSource.PlayOneShot(clip);
        }
        
        public void PlayMusic(AudioClip clip)
        {
            if (!clip  || !musicAudioSource) return;
            musicAudioSource.clip = clip;
            musicAudioSource.Play();
        }
        
        public void FadeOutMusic()
        {
            if (!musicAudioSource) return;
            StartCoroutine(FadeOut(musicAudioSource, 1f));
        }
        
        IEnumerator FadeOut(AudioSource audioSource, float fadeTime)
        {
            float startVolume = audioSource.volume;
 
            while (audioSource.volume > 0)
            {
                audioSource.volume -= startVolume * Time.deltaTime / fadeTime;
 
                yield return null;
            }
 
            audioSource.Stop();
            audioSource.volume = startVolume;
        }
        
        public void StopMusic()
        {
            musicAudioSource.Stop();
        }
    }
}