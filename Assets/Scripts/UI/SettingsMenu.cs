using Snowy.Settings;
using Snowy.UI;
using UnityEngine;

namespace UI
{
    public class SettingsMenu : MonoBehaviour
    {
        [SerializeField] private SnSelector resolutionSelector;
        [SerializeField] private SnSelector qualitySelector;
        
        private void Start()
        {
            // Load resolution options
            var resolutionNames = Settings.Instance.displaySettings.GetResolutionNames();
            resolutionSelector.SetOptions(resolutionNames);
            resolutionSelector.SetIndex(Settings.Instance.displaySettings.GetResolutionIndex());
            resolutionSelector.onValueChanged.AddListener(OnResolutionChanged);
            
            // Load quality options
            var quality = Settings.Instance.graphicSettings;
            var qualityNames = quality.ConvertAll(x => x.name).ToArray();
            qualitySelector.SetOptions(qualityNames);
            var index = quality.FindIndex(x => x.name == Settings.Instance.currentProfile.name);
            qualitySelector.SetIndex(index);
            qualitySelector.onValueChanged.AddListener(OnQualityChanged);
        }
        
        private void OnResolutionChanged()
        {
            Settings.Instance.displaySettings.SetResolution(resolutionSelector.GetSelectedOption());
        }
        
        private void OnQualityChanged()
        {
            Settings.Instance.SelectProfile(qualitySelector.GetSelectedOption());
        }
    }
}