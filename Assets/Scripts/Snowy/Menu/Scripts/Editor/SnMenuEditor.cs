using Snowy.Utils;
using UnityEditor;
using UnityEngine;

namespace Snowy.Menu
{
    [CustomEditor(typeof(SnMenu))]
    public class SnMenuEditor : Editor
    {
        private SnMenu m_menu;
        private SerializedProperty m_buttonLayoutGroup;
        private SerializedProperty m_titleLayoutGroup;
        private SerializedProperty m_moveCameraToMenu;
        private SerializedProperty m_camPosition;
        private SerializedProperty m_canSavePreviousMenu;
        
        private void OnEnable()
        {
            m_menu = (SnMenu) target;
            m_buttonLayoutGroup = serializedObject.FindProperty("buttonLayoutGroup");
            m_titleLayoutGroup = serializedObject.FindProperty("titleLayoutGroup");
            m_canSavePreviousMenu = serializedObject.FindProperty("canSavePreviousMenu");
            m_moveCameraToMenu = serializedObject.FindProperty("moveCameraToMenu");
            m_camPosition = serializedObject.FindProperty("camPosition");
        }
        
        public override void OnInspectorGUI()
        {
            if (m_menu == null)
                return;
            
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Menu", SnGUI.skin.titleStyle);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_canSavePreviousMenu);
            EditorGUILayout.PropertyField(m_moveCameraToMenu);
            if (m_moveCameraToMenu.boolValue)
                EditorGUILayout.PropertyField(m_camPosition);
            serializedObject.ApplyModifiedProperties();
        }
    }
}