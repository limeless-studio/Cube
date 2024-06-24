using System;
using System.Collections;
using Interaction;
using TMPro;
using UnityEngine;

namespace Interface.Elements
{
    public class InteractUI : Element
    {
        [SerializeField] private GameObject interactUI;
        [SerializeField] private TMP_Text interactText;
        [SerializeField] private Transform interactCrosshair;
        
        private bool isShowing;

        private void Awake()
        {
            HideInteractUI();
        }


        protected override void Tick()
        {
            base.Tick();
            if (character)
            {
                if (character.Interactor.interactable != null) 
                {
                    if (!isShowing) ShowInteractUI(character.Interactor.interactable);
                } 
                else 
                {
                    if (isShowing) HideInteractUI();
                }
            }
        }
        
        private void ShowInteractUI(Interactable interactable)
        {
            isShowing = true;
            interactUI.SetActive(true);
            interactText.text = interactable.action.Prompt;
            StartCoroutine(ScaleUp(3f));
        }
        
        private void HideInteractUI()
        {
            isShowing = false;
            interactUI.SetActive(false);
            StartCoroutine(ScaleDown());
        }
        
        IEnumerator ScaleUp(float multiplier)
        {
            float scaleUpStartTime = Time.time;
            var start = interactCrosshair.transform.localScale;
            var duration = 0.1f;
            while (Time.time - scaleUpStartTime < duration)
            {
                interactCrosshair.transform.localScale = Vector3.Lerp(start, Vector3.one * multiplier, (Time.time - scaleUpStartTime) / duration);
                yield return null;
            }
        }

        IEnumerator ScaleDown()
        {
            float scaleDownStartTime = Time.time;
            var start = interactCrosshair.transform.localScale;
            var duration = 0.1f;
            while (Time.time - scaleDownStartTime < duration)
            {
                interactCrosshair.transform.localScale =
                    Vector3.Lerp(start, Vector3.one, (Time.time - scaleDownStartTime) / duration);
                yield return null;
            }
        }
    }
}