using Snowy.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Snowy.NotificationManager.Extra
{
    public class SnSubmitNotification : SnNotificationObject
    {
        [SerializeField] private SnButton submitButton;
        [SerializeField] private SnButton cancelButton;
        
        public UnityEvent OnSubmit;
        public UnityEvent OnCancel;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            submitButton.OnClick.AddListener(Submit);
            cancelButton.OnClick.AddListener(Cancel);
        }
        
        public void Submit()
        {
            OnSubmit.Invoke();
            HideNotification();
        }

        public void Cancel()
        {
            OnCancel.Invoke();
            HideNotification();
        }
    }
}