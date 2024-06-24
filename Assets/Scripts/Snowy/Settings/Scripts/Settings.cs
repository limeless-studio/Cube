using System.Collections.Generic;
using UnityEngine;

namespace Snowy.Settings
{
    public class Settings : MonoBehaviour
    {
        private static Settings _instance;
        
        public static Settings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<Settings>();
                }
                return _instance;
            }
        }
        
        public DisplaySettings displaySettings;
        public List<GraphicProfile> graphicSettings;
        public AudioSettings audioSettings;
        
        public GraphicProfile currentProfile;
        
        public static event System.Action<GraphicProfile> OnProfileChanged;
        
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            displaySettings ??= new DisplaySettings();
            
            displaySettings.Load();
            foreach (GraphicProfile profile in graphicSettings) profile.Load();
            
            if (PlayerPrefs.HasKey("CurrentProfile"))
            {
                SelectProfile(PlayerPrefs.GetString("CurrentProfile"));
            }
            else {
                SelectProfile(graphicSettings[0].name);
            }
        }

        private void Start()
        {
            audioSettings?.Load();
        }

        #region Graphic Settings
        
        public void AddProfile(GraphicProfile profile)
        {
            graphicSettings.Add(profile);
        }
        public GraphicProfile GetProfile(string profileName)
        {
            return graphicSettings.Find(x => x.name == profileName);
        }
        
        public void SelectProfile(string profileName)
        {
            GraphicProfile profile = GetProfile(profileName);
            if (profile != null)
            {
                profile.Apply();
                currentProfile = profile;
                PlayerPrefs.SetString("CurrentProfile", profileName);
                OnProfileChanged?.Invoke(profile);
            }
        }
        
        #endregion
        
        #region Audio Settings
        
        public void AddAudioProfile(AudioProfile profile)
        {
            if (audioSettings) audioSettings.audioProfiles.Add(profile);
        }
        
        #endregion
    }
}