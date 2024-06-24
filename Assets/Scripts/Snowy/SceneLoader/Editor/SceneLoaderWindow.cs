using Snowy.Utils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Snowy.SceneLoader
{
    public class SceneLoaderWindow : EditorWindow
    {
        private const string scenePath = "Assets/Scenes/";
        
        // List of scenes to load
        private string[] m_sceneNames;
        
        Vector2 scrollPos;
        
        [MenuItem("Snowy/Scene Loader")]
        private static void ShowWindow()
        {
            GetWindow<SceneLoaderWindow>("Scene Loader");
        }
        
        private void OnEnable()
        {
            m_sceneNames = SnEditorUtils.GetSceneNames(scenePath);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Scene Loader", SnGUI.skin.titleStyle);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();
            
            // Scroll
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            
            // Make a list of buttons for each scene
            foreach (string sceneName in m_sceneNames)
            {
                var sName = sceneName.Replace(scenePath, "").Replace(".unity", "").Replace("/", " -> ");
                if (GUILayout.Button(sName, GUILayout.Height(30)))
                {
                    LoadScene(sceneName);
                }
                
                // Add a separator between buttons
                if (sceneName != m_sceneNames[m_sceneNames.Length - 1])
                {
                    EditorGUILayout.Space();
                }
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        public static void LoadScene(string sceneName)
        {
            if (EditorApplication.isPlaying)
            { 
                // Load the scene in play mode
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            }
            else
            {
                // If need saving ask whether to save or no, or cancel
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    // Load the scene in edit mode
                    EditorSceneManager.OpenScene(sceneName);
                }
            }
        }
        
        // On Folder refresh, update the scene names
        private void OnProjectChange()
        {
            m_sceneNames = SnEditorUtils.GetSceneNames(scenePath);
        }
    }
}