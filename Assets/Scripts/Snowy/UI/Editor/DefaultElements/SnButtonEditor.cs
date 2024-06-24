using Snowy.Utils;
using UnityEditor;
using UnityEngine;

namespace Snowy.UI.DefaultElements
{
    [CustomEditor(typeof(SnButton), true)]
    public class SnButtonEditor : SnSelectableEditor
    {
        private SnButton m_button;
        
        private SerializedProperty m_playAudio;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_button = (SnButton) target;
            m_playAudio = serializedObject.FindProperty("playAudio");
        }

        public override void OnInspectorGUI()
        {
            if (m_button == null) { return; }
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Button Settings", SnGUI.skin.titleStyle);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            // Serialize the onclick serialized property
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onClick"));
            EditorGUILayout.PropertyField(m_playAudio);
            serializedObject.ApplyModifiedProperties();
            
            base.OnInspectorGUI();
        }
    }
}