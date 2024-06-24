using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Snowy.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class SnSelectable : UIBehaviour,
        IMoveHandler,
        IPointerDownHandler, IPointerUpHandler,
        IPointerEnterHandler, IPointerExitHandler,
        ISelectHandler, IDeselectHandler, IEffectsManager
    {
        public EffectsGroup onHoverEffects = new(true);
        public EffectsGroup onClickEffects = new(true);
        public EffectsGroup onSelectEffects = new(true);
        private bool m_isHovered;
        private bool m_isSelected;
        private bool m_isPressed;
        private bool m_interactable = true;
        private bool m_enableEffects = true;
        
        // OnHover, OnClick
        public event Action OnClick;
        public event Action OnHover;
        
        public CanvasGroup CanvasGroup => m_canvasGroup;
        public Graphic TargetGraphic => m_targetGraphic;
        public Transform Transform => transform;
        public MonoBehaviour Mono => this;
        
        public bool Interactable
        {
            get => m_interactable;
            set
            {
                if (m_targetGraphic)
                {
                    m_canvasGroup.alpha = value ? 1 : 0.2f;
                }
                m_interactable = value;
            }
        }
        
        public bool EnableEffects
        {
            get => m_enableEffects;
            set => m_enableEffects = value;
        }
        
        private CanvasGroup m_canvasGroup;
        private Graphic m_targetGraphic;
        
        protected override void Awake()
        {
            m_canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            m_targetGraphic = GetComponentInChildren<Graphic>();
            
            if (onHoverEffects == null) onHoverEffects = new EffectsGroup(true);
            if (onClickEffects == null) onClickEffects = new EffectsGroup(true);
            if (onSelectEffects == null) onSelectEffects = new EffectsGroup(true);
            
            onHoverEffects.Initialize(this);
            onClickEffects.Initialize(this);
            onSelectEffects.Initialize(this);
            
            base.Awake();
        }
        
        # if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (onHoverEffects == null) onHoverEffects = new EffectsGroup(true);
            if (onClickEffects == null) onClickEffects = new EffectsGroup(true);
            if (onSelectEffects == null) onSelectEffects = new EffectsGroup(true);
            
            if (m_canvasGroup == null) m_canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            if (m_targetGraphic == null) m_targetGraphic = GetComponentInChildren<Graphic>();
        }
        # endif
        
        public void OnMove(AxisEventData eventData)
        {
            // TODO: Implement
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            m_isPressed = true;
            if (!m_enableEffects) return;
            StopAllCoroutines();
            StartCoroutine(onClickEffects.Apply(this));
            
            if (EventSystem.current.currentSelectedGameObject != gameObject)
            {
                EventSystem.current.SetSelectedGameObject(gameObject);
            }
        }

        protected override void OnDisable()
        {
            m_isHovered = false;
            m_isSelected = false;
            m_isPressed = false;
            
            // Disable all effects
            StopAllCoroutines();
            onHoverEffects.ImmediateCancel(this);
            onClickEffects.ImmediateCancel(this);
            onSelectEffects.ImmediateCancel(this);
            base.OnDisable();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnClick?.Invoke();

            m_isPressed = false;
            if (!m_enableEffects) return;
            if (!m_isHovered && m_isSelected)
            {
                StopAllCoroutines();
                StartCoroutine(onSelectEffects.Apply(this));
            }
            else if (m_isHovered)
            {
                StopAllCoroutines();
                StartCoroutine(onHoverEffects.Apply(this));
            }
            else
            {
                StopAllCoroutines();
                StartCoroutine(onClickEffects.Cancel(this));
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_isHovered = true;
            if (!m_enableEffects) return;
            if (!m_isPressed)
            {
                StopAllCoroutines();
                StartCoroutine(onHoverEffects.Apply(this));
                OnHover?.Invoke();
            }
        }
 
        public void OnPointerExit(PointerEventData eventData)
        {
            m_isHovered = false;
            if (!m_enableEffects) return;
            if (!m_isPressed && !m_isSelected)
            {
                StopAllCoroutines();
                StartCoroutine(onHoverEffects.Cancel(this));
            } else if (m_isSelected)
            {
                StopAllCoroutines();
                StartCoroutine(onHoverEffects.Cancel(this));
                StartCoroutine(onSelectEffects.Apply(this));
            } else if (m_isPressed)
            {
                StopAllCoroutines();
                StartCoroutine(onHoverEffects.Cancel(this));
                StartCoroutine(onClickEffects.Apply(this));
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            m_isSelected = true;
            if (!m_enableEffects) return;
            if (!m_isPressed)
            {
                StopAllCoroutines();
                StartCoroutine(onSelectEffects.Apply(this));
            }
        }

        public void OnDeselect(BaseEventData eventData)
        {
            m_isSelected = false;
            if (!m_enableEffects) return;
            if (!m_isPressed && m_isHovered)
            {
                StopAllCoroutines();
                StartCoroutine(onHoverEffects.Apply(this));
            }
            else if (m_isPressed)
            {
                StopAllCoroutines();
                StartCoroutine(onClickEffects.Apply(this));
            }
            else
            {
                StopAllCoroutines();
                StartCoroutine(onSelectEffects.Cancel(this));
            }
        }
        
        public virtual bool IsInteractable()
        {
            return m_interactable;
        }
    }
}