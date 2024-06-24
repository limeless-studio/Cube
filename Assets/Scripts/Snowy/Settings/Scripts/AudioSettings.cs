using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Snowy.Settings
{
    // Audio Profiles are used to store audio settings for different parts of the game.
    [Serializable] public class AudioProfile
    {
        [HideInInspector] public bool createNew = false;
        public string newName = "New Profile";
        public AudioMixerGroup mixerGroup = null;
        public float volume = 1;
    }
    
    // Snowy/Settings/AudioSettings
    [CreateAssetMenu(fileName = "AudioSettings", menuName = "Snowy/Settings/AudioSettings")]
    public class AudioSettings : ScriptableObject
    {
        public AudioMixer masterMixer;
        public string masterVolumeParameter = "MasterVolume";
        public float masterVolume = 1;
        public List<AudioProfile> audioProfiles;
        
        public void Load()
        {
            masterVolume = PlayerPrefs.HasKey("MasterVolume") ? PlayerPrefs.GetFloat("MasterVolume") : 1;
            foreach (AudioProfile profile in audioProfiles)
            {
                SetProfileVolume(profile, PlayerPrefs.HasKey(profile.newName + "Volume") ? PlayerPrefs.GetFloat(profile.newName + "Volume") : 1);
            }
            
            SetMasterVolume(masterVolume);
        }
        
        public void SetMasterVolume(float value)
        {
            masterVolume = value;
            if (value != 0)
                masterMixer.SetFloat(masterVolumeParameter, Mathf.Log10(value) * 20);
            else
                masterMixer.SetFloat(masterVolumeParameter, -80);
            PlayerPrefs.SetFloat("MasterVolume", value);
        }
        
        public void SetProfileVolume(AudioProfile profile, float value)
        {
            profile.volume = value;
            if (value != 0)
                profile.mixerGroup.audioMixer.SetFloat(profile.newName + "Volume", Mathf.Log10(value) * 20);
            else
                profile.mixerGroup.audioMixer.SetFloat(profile.newName + "Volume", -80);
            PlayerPrefs.SetFloat(profile.newName + "Volume", value);
        }
        
        public void SetProfileVolume(string profileName, float value)
        {
            if (profileName == "Master")
            {
                SetMasterVolume(value);
                return;
            }
            
            AudioProfile profile = audioProfiles.Find(x => x.newName == profileName);
            if (profile != null)
            {
                SetProfileVolume(profile, value);
            }
        }
        
        public float GetProfileVolume(string profileName)
        {
            if (profileName == "Master") return masterVolume;
            AudioProfile profile = audioProfiles.Find(x => x.newName == profileName);
            return profile != null ? profile.volume : 1;
        }
    }
}