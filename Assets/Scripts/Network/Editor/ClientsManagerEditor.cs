using System.Linq;
using Snowy.Utils;
using UnityEngine;

namespace Network.Client.Editor
{
    [UnityEditor.CustomEditor(typeof(ClientsManager))]
    public class ClientsManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var clientsManager = (ClientsManager) target;
            SnEditorGUI.DrawTitle("Clients Manager");
            // Draw all clients as table, ID, Username, SteamId
            SnEditorGUI.BeginSection("Clients");
            UnityEditor.EditorGUILayout.BeginHorizontal();
            UnityEditor.EditorGUILayout.LabelField("ID", UnityEditor.EditorStyles.boldLabel, GUILayout.Width(50));
            UnityEditor.EditorGUILayout.LabelField("Username", UnityEditor.EditorStyles.boldLabel);
            UnityEditor.EditorGUILayout.LabelField("SteamId", UnityEditor.EditorStyles.boldLabel, GUILayout.Width(100));
            UnityEditor.EditorGUILayout.EndHorizontal();
            
            foreach (var client in clientsManager.GetClients())
            {
                UnityEditor.EditorGUILayout.BeginHorizontal();
                UnityEditor.EditorGUILayout.LabelField(client.ClientInfo.ClientId.ToString(), GUILayout.Width(50));
                UnityEditor.EditorGUILayout.LabelField(client.ClientInfo.Username);
                UnityEditor.EditorGUILayout.LabelField(client.ClientInfo.SteamId.ToString(), GUILayout.Width(100));
                UnityEditor.EditorGUILayout.EndHorizontal();
            }
            
            // Separator
            UnityEditor.EditorGUILayout.Space();
            UnityEditor.EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            UnityEditor.EditorGUILayout.LabelField("Total clients", clientsManager.GetClients().Count().ToString());
            SnEditorGUI.EndSection();
        }
    }
}