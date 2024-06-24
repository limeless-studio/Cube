using UnityEngine;

namespace Network.Client.Editor
{
    [UnityEditor.CustomEditor(typeof(LocalLobbyManager))]
    public class LocalManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var localManager = (LocalLobbyManager) target;
            if (UnityEditor.EditorApplication.isPlaying)
            {
                // Buttons
                if (GUILayout.Button("Create Lobby"))
                {
                    localManager.CreateLobby();
                }
                
                if (GUILayout.Button("Join Random"))
                {
                    localManager.JoinRandom();
                }
            }
            else
            {
                UnityEditor.EditorGUILayout.Space();
                UnityEditor.EditorGUILayout.HelpBox("You can only create/join lobbies while in play mode", UnityEditor.MessageType.Info);
            }
        }
    }
}