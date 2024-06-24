using System;
using System.Collections.Generic;
using Snowy.Audio;
using Snowy.Menu;
using Snowy.UI.interfaces;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Snowy.UI
{
    [Serializable] public class SnButtonEvent : UnityEvent { }
    
    public class SnButton : SnSelectable, IPointerClickHandler, ISubmitHandler
    {
        [SerializeField] private SnButtonEvent onClick = new SnButtonEvent();
        [SerializeField] private bool playAudio = true;
        
        public SnButtonEvent OnClick
        {
            get => onClick;
            set => onClick = value;
        }

        protected override void Awake()
        {
            base.Awake();

            base.OnHover += OnHoverAudio;
            base.OnClick += OnClickAudio;
        }
        
        private void Press()
        {
            if (!IsActive() || !IsInteractable())
                return;

            UISystemProfilerApi.AddMarker("Button.onClick", this);
            onClick.Invoke();
        }
        
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            Press();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            Press();

            // if we get set disabled during the press
            // don't run the coroutine.
            if (!IsActive() || !IsInteractable())
                return;
        }
        
        private void OnHoverAudio()
        {
            if (playAudio)
            {
                if (SoundManager.Instance)
                {
                    SoundManager.Instance.PlayUISound(SoundManager.Instance.menuClipsTheme.buttonHover);
                }
            }
        }
        
        private void OnClickAudio()
        {
            if (playAudio)
            {
                if (SoundManager.Instance)
                {
                    if (IsInteractable()) SoundManager.Instance.PlayUISound(SoundManager.Instance.menuClipsTheme.buttonClick);
                    else SoundManager.Instance.PlayUISound(SoundManager.Instance.menuClipsTheme.buttonDisabledClick);
                }
            }
        }
    }
}