using Snowy.Utils;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public class MinigamesManagerWindow : EditorWindow
    {
        private MinigameData[] minigameData = new MinigameData[0];
        Vector2 scrollPosition;
        
        [MenuItem("Window/Minigames Manager")]
        public static void ShowWindow()
        {
            GetWindow<MinigamesManagerWindow>("Minigames Manager");
        }
        
        private void OnEnable()
        {
            // in the presets folder, find all the minigame data assets
            GetMiniGameData();
        }
        
        private void GetMiniGameData()
        {
            var guids = AssetDatabase.FindAssets("t:MinigameData", new[] {"Assets/Presets"});
            minigameData = new MinigameData[guids.Length];
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var data = AssetDatabase.LoadAssetAtPath<MinigameData>(path);
                
                // if the id is not set or already exists, generate a new id
                if (data.id == -1 || !IsIdUnique(data.id))
                {
                    data.id = GenerateId();
                    EditorUtility.SetDirty(data);
                }
                
                minigameData[i] = data;
            }
        }

        private void OnGUI()
        {
            if (minigameData == null) return;
            SnEditorGUI.DrawTitle("Minigame Data");
            
            // Scroll view
            EditorGUILayout.BeginVertical("box");
            if (minigameData.Length == 0)
            {
                EditorGUILayout.HelpBox("No minigame data found", MessageType.Info);
            }
            else
            {
                // Scroll view
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                
                for (var i = 0; i < minigameData.Length; i++)
                {
                    var data = minigameData[i];
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.ObjectField(data, typeof(MinigameData), false);
                    if (GUILayout.Button("Select"))
                    {
                        // Ping the object in the project window
                        EditorGUIUtility.PingObject(data);
                        // select the object in the project window
                        Selection.activeObject = data;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
            
            // Refrsh and create buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh"))
            {
                GetMiniGameData();
            }
            
            if (GUILayout.Button("Create"))
            {
                var data = CreateInstance<MinigameData>();
                data.id = GenerateId();
                var path = EditorUtility.SaveFilePanelInProject("Save Minigame Data", "New Minigame Data", "asset", "Save Minigame Data");
                if (path.Length != 0)
                {
                    AssetDatabase.CreateAsset(data, path);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    GetMiniGameData();
                }
            }
            
            if (GUILayout.Button("Regenerate Ids"))
            {
                RegenerateIds();
            }
            EditorGUILayout.EndHorizontal();
        }
        
        private bool IsIdUnique(int id)
        {
            foreach (var data in minigameData)
            {
                if (!data) continue;
                if (data.id == id)
                {
                    return false;
                }
            }
            return true;
        }
        
        private int GenerateId()
        {
            var id = 0;
            foreach (var data in minigameData)
            {
                if (!data) continue;
                if (data.id > id)
                {
                    id = data.id;
                }
            }
            return id + 1;
        }

        private void RegenerateIds()
        {
            foreach (var data in minigameData)
            {
                if (!data) continue;
                data.id = -1;
            }
            
            GetMiniGameData();
        }
    }
}