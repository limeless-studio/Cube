using Snowy.Utils;
using UnityEditor;
using UnityEngine;

namespace Snowy.NotificationManager
{
    [CustomEditor(typeof(SnNotificationsContainer))]
    public class SnNotificationContainerEditor : Editor
    {
        private SerializedProperty _notificationTypes;
        private SnNotificationsContainer _container;

        private void OnEnable()
        {
            _notificationTypes = serializedObject.FindProperty("notificationTypes");
            
            _container = (SnNotificationsContainer) target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Notification Types", SnGUI.skin.titleStyle);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            serializedObject.Update();

            EditorGUILayout.PropertyField(_notificationTypes, true);

            serializedObject.ApplyModifiedProperties();
            
            // A button to add a new notification type
            if (GUILayout.Button("Add Notification Type"))
            {
                SnNotificationsContainer container = (SnNotificationsContainer) target;
                ArrayUtility.Add(ref container.notificationTypes, new NotificationType());
            }
            
            // Bake button
            if (GUILayout.Button("Bake"))
            {
                string[] names = new string[_container.notificationTypes.Length];
                for (int i = 0; i < _container.notificationTypes.Length; i++)
                {
                    names[i] = _container.notificationTypes[i].name;
                }
                
                NotificationTypesBuilder.BuildNotificationTypesEnum(names);
                
                Debug.Log("Baked");
            }
        }
    }
}