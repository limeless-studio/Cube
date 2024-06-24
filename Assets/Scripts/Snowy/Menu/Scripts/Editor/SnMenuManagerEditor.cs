using System.Linq;
using Snowy.Utils;
using UnityEditor;
using UnityEngine;

namespace Snowy.Menu
{
    [CustomEditor(typeof(SnMenuManager))]
    public class SnMenuManagerEditor : Editor
    {
        private SerializedProperty m_fetchOnAwake;
        private SerializedProperty m_defaultMenuID;
        private SerializedProperty m_cameraTransform;
        private SerializedProperty m_cameraDuration;
        private SerializedProperty m_debug;
        SnMenuManager m_manager;
        
        private void OnEnable()
        {
            m_fetchOnAwake = serializedObject.FindProperty("fetchOnAwake");
            m_defaultMenuID = serializedObject.FindProperty("defaultMenuID");
            m_cameraTransform = serializedObject.FindProperty("cameraTransform");
            m_cameraDuration = serializedObject.FindProperty("cameraDuration");
            m_debug = serializedObject.FindProperty("debug");
            
            m_manager = (SnMenuManager) target;
        }
        
        public override void OnInspectorGUI()
        {
            if (m_manager == null)
                return;
            
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Menu Manager", SnGUI.skin.titleStyle);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();
            
            // Draw the fetchOnAwake and defaultMenuID fields
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_fetchOnAwake);
            
            // Draw a dropdown for the default menu ID
            var menus = m_manager.GetMenusList();
            var menuNames = menus.Select(menu => menu.MenuName).ToArray();
            var defaultMenuID = m_defaultMenuID.intValue;
            var index = menus.FindIndex(menu => menu.MenuID == defaultMenuID);
            if (index == -1)
                index = 0;
            
            if (menus.Count == 0)
            {
                EditorGUILayout.Popup("Default Menu", 0, new string[] {"No menus found"});
            }
            else
            {
                index = EditorGUILayout.Popup("Default Menu", index, menuNames);
            }
            m_defaultMenuID.intValue = menus.Find(menu => menu.MenuName == menuNames[index])?.MenuID ?? -1;
            
            EditorGUILayout.PropertyField(m_cameraTransform);
            EditorGUILayout.PropertyField(m_cameraDuration);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();
            
            // Draw a toggle for debug mode
            EditorGUILayout.PropertyField(m_debug);
            EditorGUILayout.Space();

            if (m_debug.boolValue)
            {
                // Draw a list of all menus
                // Box
                EditorGUILayout.BeginVertical(SnGUI.skin.boxStyle);
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                EditorGUILayout.LabelField("Menus", SnGUI.skin.titleStyle);
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                EditorGUILayout.Space();
                if (menus.Count > 0)
                {
                    foreach (var menu in menus.ToList())
                    {
                        if (!menu)
                            continue;
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(menu.MenuName + $"[{menu.MenuID}]", GUILayout.Width(200));
                        // Draw a disabled game object field
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.ObjectField(menu.gameObject, typeof(GameObject), true);
                        EditorGUI.EndDisabledGroup();
                        if (GUILayout.Button("X", GUILayout.Width(20)))
                        {
                            // confirm removal
                            if (EditorUtility.DisplayDialog("Remove Menu", $"Are you sure you want to remove {menu.MenuName}?", "Yes", "No"))
                            {
                                // register the undo
                                Undo.RecordObject(m_manager, "Remove Menu");
                                m_manager.RemoveMenu(menu.MenuID);
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                } else
                {
                    EditorGUILayout.LabelField("No menus found, try fetching menus, or add a new menu", GUILayout.Width(200));
                }
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.Space();
            // Draw a button to fetch menus
            if (GUILayout.Button("Fetch Menus"))
            {
                m_manager.FetchMenus();
            }
            
            if (GUILayout.Button("Regenerate Menu IDs"))
            {
                m_manager.RegenerateMenuIDs();
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}