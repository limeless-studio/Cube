using System;
using UnityEngine;
using UnityEngine.UI;

namespace Snowy.NotificationManager
{
    [Serializable] public class NotificationType
    {
        public string name;
        public int poolSize;
        public HorizontalOrVerticalLayoutGroup layoutGroupPrefab;
        public SnNotificationObject notificationObject;
    }
    
    [CreateAssetMenu(fileName = "NotificationsContainer", menuName = "Snowy/SnNotify/NotificationsContainer")]
    public class SnNotificationsContainer : ScriptableObject
    {
        public NotificationType[] notificationTypes;

        public SnNotificationObject GetNotification(NotificationTypeNames type)
        {
            foreach (var notificationType in notificationTypes)
            {
                if (notificationType.name == type.ToString())
                {
                    return notificationType.notificationObject;
                }
            }

            return null;
        }
    }
}