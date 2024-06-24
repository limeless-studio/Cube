using System;
using System.Collections.Generic;
using Snowy.Utils;
using UnityEngine;

namespace Snowy.NotificationManager
{
    public class SnNotificationPool
    {
        private readonly SnNotificationObject[] m_notifications;
        private int m_index;

        public SnNotificationPool(SnNotificationObject[] notifications)
        {
            m_notifications = notifications;
            m_index = 0;
        }

        public SnNotificationObject GetNext()
        {
            SnNotificationObject notification = m_notifications[m_index];
            m_index = (m_index + 1) % m_notifications.Length;
            return notification;
        }
    }

    public class SnNotificationManager : MonoSingleton<SnNotificationManager>
    {
        public SnNotificationsContainer notificationsContainer;
        
        // Pool
        private Dictionary<NotificationTypeNames, SnNotificationPool> m_pools = new ();
        
        protected override void Awake()
        {
            m_pools = new Dictionary<NotificationTypeNames, SnNotificationPool>();
            foreach (var notificationType in notificationsContainer.notificationTypes)
            {
                string typeName = notificationType.name;
                int poolSize = notificationType.poolSize;
                SnNotificationObject[] notifications = new SnNotificationObject[poolSize];
                // Init the layout group
                Transform layoutGroup = transform;
                if (notificationType.layoutGroupPrefab)
                {
                    layoutGroup = Instantiate(notificationType.layoutGroupPrefab, transform).transform;
                    layoutGroup.localScale = notificationType.layoutGroupPrefab.transform.localScale;
                    layoutGroup.localPosition = notificationType.layoutGroupPrefab.transform.localPosition;
                    layoutGroup.localRotation = notificationType.layoutGroupPrefab.transform.localRotation;
                }
                
                for (int i = 0; i < poolSize; i++)
                {
                    notifications[i] = Instantiate(notificationType.notificationObject, layoutGroup);
                    notifications[i].transform.localScale = notificationType.notificationObject.transform.localScale;
                    notifications[i].transform.localPosition = notificationType.notificationObject.transform.localPosition;
                    notifications[i].transform.localRotation = notificationType.notificationObject.transform.localRotation;
                    notifications[i].gameObject.SetActive(false);
                }
                m_pools.Add((NotificationTypeNames) Enum.Parse(typeof(NotificationTypeNames), typeName), new SnNotificationPool(notifications));
            }
            
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }
        
        public void ShowNotification<T>(NotificationTypeNames type, string title, string content = "", float duration = 3f, T data = null) where T : NotificationData
        {
            SnNotification<T> snNotification = new () {
                title = title,
                content = content,
                duration = duration,
                data = data
            };
            ShowNotification(type, snNotification);
        }
        
        public SnNotificationObject ShowNotification(NotificationTypeNames type, string title, string content = "", float duration = 3f)
        {
            SnNotification<NotificationData> snNotification = new () {
                title = title,
                content = content,
                duration = duration,
                data = null
            };
            return ShowNotification(type, snNotification);
        }

        public SnNotificationObject ShowNotification<T>(NotificationTypeNames type, SnNotification<T> notification) where T : NotificationData
        {
            // Get the pool
            SnNotificationPool pool = m_pools[type];
            // Get the next notification
            SnNotificationObject notificationObject = pool.GetNext();
            // Show the notification
            notificationObject.ShowNotification(notification);
            return notificationObject;
        }
    }
}