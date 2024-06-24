using Snowy.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Snowy.Settings
{
    public class PlayerRefSliderController : MonoBehaviour
    {
        [SerializeField] private string profileName;
        [SerializeField] private float multiplier = 1;
        [SerializeField] private float defaultValue = 1;
        [SerializeField] private Slider slider;
        [SerializeField] private TMP_Text text;
        [SerializeField] private SnButton leftButton;
        [SerializeField] private SnButton rightButton;
        
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
            
            if (slider == null) return;
            if (PlayerPrefs.HasKey(profileName))
            {
                slider.value = PlayerPrefs.GetFloat(profileName, defaultValue) / multiplier;
            }
            else {
                slider.value = defaultValue;
                PlayerPrefs.SetFloat(profileName, defaultValue);
            }
            slider.onValueChanged.AddListener(OnValueChanged);
            UpdateSlider();
        }
        
        private void OnValueChanged(float value)
        {
            PlayerPrefs.SetFloat(profileName, value * multiplier);
            if (text) text.text = Mathf.RoundToInt(slider.value * multiplier).ToString();
            if (leftButton) leftButton.Interactable = value > 0;
            if (rightButton) rightButton.Interactable = value < 1;
        }
        
        private void UpdateSlider()
        {
            slider.value = PlayerPrefs.GetFloat(profileName, defaultValue) / multiplier;
            if (text) text.text = Mathf.RoundToInt(slider.value * multiplier).ToString();
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