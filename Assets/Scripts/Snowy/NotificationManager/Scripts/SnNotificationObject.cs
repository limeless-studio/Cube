using System.Collections;
using Snowy.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Snowy.NotificationManager
{
    [RequireComponent(typeof(CanvasGroup))]
    public class SnNotificationObject : MonoBehaviour, IEffectsManager
    {
        [SerializeField] private TMP_Text title;
        [SerializeField] private bool hasContent;
        [SerializeField] private bool isContainer;
        [SerializeField] private Transform container;
        [SerializeField] private TMP_Text content;
        [SerializeField] private Image background;
        
        private bool m_isShowing;
        public bool IsShowing => m_isShowing;
        
        // Effects
        public EffectsGroup onShowEffects;
        public EffectsGroup onHideEffects;

        public Transform Transform => isContainer ? container : transform;
        public CanvasGroup CanvasGroup
        {
            get
            {
                if (m_canvasGroup == null)
                {
                    m_canvasGroup = GetComponent<CanvasGroup>();
                }

                return m_canvasGroup;
            }
        }
        public Graphic TargetGraphic => background;
        public MonoBehaviour Mono => this;
        
        private CanvasGroup m_canvasGroup;
        
        protected virtual void OnEnable()
        {
            if (onShowEffects != null)
            {
                onShowEffects.Initialize(this);
            }
            
            if (onHideEffects != null)
            {
                onHideEffects.Initialize(this);
            }

            if (title) title.text = "";
            if (hasContent) content.text = "";
        }
        
        protected void ShowNotification(float duration = 3f)
        {
            // Stop all coroutines
            StopAllCoroutines();
            // Disable the invoke
            CancelInvoke(nameof(HideNotification));
            StartCoroutine(Show(duration));
        }

        public IEnumerator Show(float duration)
        {
            m_isShowing = true;
            yield return onShowEffects.Apply(this);
            // Run the hide effects after the duration
            Invoke(nameof(HideNotification), duration);
        }
        
        public void HideNotification()
        {
            if (gameObject.activeSelf)
                StartCoroutine(Hide());
        }
        
        public IEnumerator Hide()
        {
            yield return onHideEffects.Apply(this);
            m_isShowing = false;
            onHideEffects.ImmediateCancel(this);
            gameObject.SetActive(false);
        }

        public void ShowNotification<T>(SnNotification<T> notification) where T : NotificationData
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            else if (m_isShowing)
            {
                CancelInvoke(nameof(HideNotification));
            }
            
            title.text = notification.title;
            if (hasContent) content.text = notification.content;
            ShowNotification(notification.duration);
        }

        #region Unity Editor
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (CanvasGroup == null)
            {
                m_canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            }
            
            if (background == null)
            {
                background = GetComponent<Image>();
            }
        }
        
        public void ShowPreview<T>(SnNotification<T> notification) where T : NotificationData
        {
            gameObject.SetActive(true);
            title.text = notification.title;
            if (hasContent) content.text = notification.content;
        }
#endif
        #endregion
    }
}