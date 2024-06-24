using UnityEditor;

namespace Snowy.NotificationManager.Extra
{
    [CustomEditor(typeof(SnSubmitNotification), true)]
    public class SnSubmitNotificationEditor : SnNotificationObjectEditor
    {
        private SerializedProperty m_submitButton;
        private SerializedProperty m_cancelButton;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            m_submitButton = serializedObject.FindProperty("submitButton");
            m_cancelButton = serializedObject.FindProperty("cancelButton");
        }

        protected override void DrawFields()
        {
            base.DrawFields();
            
            EditorGUILayout.PropertyField(m_submitButton);
            EditorGUILayout.PropertyField(m_cancelButton);
        }
    }
}