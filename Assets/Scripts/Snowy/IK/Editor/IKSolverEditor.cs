using System;
using Snowy.ActiveRagdoll.IK;
using UnityEditor;
using Snowy.Utils;
using Toolbox.Editor;
using UnityEngine;

namespace Snowy.IK
{
    [CustomEditor(typeof(IKSolver))]
    public class IKSolverEditor : Editor
    {
        private SerializedProperty m_ikComponents;
        private SerializedProperty m_editMode;

        private IKSolver m_solver;
        
        private void OnEnable()
        {
            m_ikComponents = serializedObject.FindProperty("ikComponents");
            m_editMode = serializedObject.FindProperty("editMode");
            
            m_solver = target as IKSolver;
        }
        
        public override void OnInspectorGUI()
        {
            // Draw default 
            SnEditorGUI.DrawDefaultScriptField(this);
            EditorGUILayout.Space();
            SnEditorGUI.DrawTitle("IK Solver");
            EditorGUILayout.Space();
            
            SnEditorGUI.BeginSection("IK Components");
            
            serializedObject.Update();
            
            // Draw all the element one by one
            if (m_ikComponents.arraySize > 0)
            {
                for (var i = 0; i < m_ikComponents.arraySize; i++)
                {
                    var element = m_ikComponents.GetArrayElementAtIndex(i);
                    SnEditorGUI.InlineEditor(element, typeof(IKComponent));
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No IK Components found", MessageType.Info);
            }
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Fetch IK Components"))
            {
                m_solver.FetchIKComponents();
            }
            
            SnEditorGUI.EndSection();
            
            SnEditorGUI.BeginSection("Edit Mode");
            EditorGUILayout.PropertyField(m_editMode);
            SnEditorGUI.EndSection();
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}