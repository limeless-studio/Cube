using Snowy.Utils;

namespace Network.Client.Editor
{
    [UnityEditor.CustomEditor(typeof(ClientInfo))]
    public class ClientInfoEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var clientInfo = (ClientInfo) target;
            SnEditorGUI.DrawTitle("Client Info");
            
            SnEditorGUI.BeginSection("Client Info");
            UnityEditor.EditorGUILayout.LabelField("Username", clientInfo.Username);
            UnityEditor.EditorGUILayout.LabelField("SteamId", clientInfo.SteamId.ToString());
            UnityEditor.EditorGUILayout.LabelField("ClientId", clientInfo.ClientId.ToString());
            UnityEditor.EditorGUILayout.LabelField("Is Local", clientInfo.IsLocal.ToString());
            SnEditorGUI.EndSection();
        }
    }
}