using Snowy.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Snowy.Settings
{
    public class VolumeController : MonoBehaviour
    {
        [SerializeField] private SnButton leftButton;
        [SerializeField] private SnButton rightButton;
        [SerializeField] private TMP_Text text;
        [SerializeField] private string profileName;
        [SerializeField] private Slider slider;
        
        private void Start()
        {
            if (leftButton != null)
            {
                leftButton.OnClick.AddListener(() => SubtractVolume(.1f));
            }
            
            if (rightButton != null)
            {
                rightButton.OnClick.AddListener(() => AddVolume(.1f));
            }
            
            slider.onValueChanged.AddListener(OnValueChanged);
            UpdateSlider();
        }
        
        private void OnValueChanged(float value)
        {
            Settings.Instance.audioSettings.SetProfileVolume(profileName, value);
            if (text) text.text = Mathf.RoundToInt(value * 100) + "%";
            if (leftButton) leftButton.Interactable = value > 0;
            if (rightButton) rightButton.Interactable = value < 1;
        }
        
        private void UpdateSlider()
        {
            slider.value = Settings.Instance.audioSettings.GetProfileVolume(profileName);
            if (text) text.text = Mathf.RoundToInt(slider.value * 100) + "%";
        }
        
        public void AddVolume(float value)
        {
            slider.value += value;
        }
        
        public void SubtractVolume(float value)
        {
            slider.value -= value;
        }
    }
}