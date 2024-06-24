using UnityEngine;

namespace Snowy.NotificationManager.Tests
{
    public class NotificationTest : MonoBehaviour
    {
        [SerializeField] private NotificationTypeNames notificationType;
        [SerializeField] private string title;
        [SerializeField] private string content;
        [SerializeField] private float duration = 3f;
        
        public void ShowNotification()
        {
            SnNotificationManager.Instance.ShowNotification(notificationType, title, content, duration);
        }
    }
}