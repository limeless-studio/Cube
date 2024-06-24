using System;

namespace Snowy.NotificationManager
{
    [Serializable] public class SnNotification<T> where T : NotificationData
    {
        public string title;
        public string content;
        public float duration;
        public T data = null;
    }
}