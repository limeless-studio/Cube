using UnityEditor;
using UnityEngine;

namespace Snowy.NotificationManager
{
    public static class NotificationTypesBuilder
    {
        // Takes a list and generates a code snippet for the NotificationTypes.cs file 
        // enum NotificationTypeNames: string
        /*
         * namespace NotificationSystem
         *  {
         *     public enum NotificationTypeNames {
         *        Popup,
         *        Toast,
         *        Banner
         *    }
         *  }
         */

        public static string RootPath
        {
            get
            {
                var g = AssetDatabase.FindAssets($"t:Script {nameof(NotificationTypesBuilder)}");
                if (g.Length == 0)
                {
                    Debug.LogError("NotificationTypesBuilder.cs not found in the project");
                    return null;
                }
                
                var path = AssetDatabase.GUIDToAssetPath(g[0]);
                return path.Replace($"{nameof(NotificationTypesBuilder)}.cs", "NotificationTypeNames.cs")
                    .Replace("/Editor/", "/Scripts/");
            }
        }
        
        public static void BuildNotificationTypesEnum(string[] notificationTypes) 
        {
            string enumString = "namespace Snowy.NotificationManager\n{\n\tpublic enum NotificationTypeNames {\n";
            for (int i = 0; i < notificationTypes.Length; i++)
            {
                enumString += $"\t\t{notificationTypes[i]},\n";
            }
            enumString += "\t}\n}";
            
            // Get current path
            // Write to file
            System.IO.File.WriteAllText(RootPath, enumString);
            
            // Refresh the editor
            AssetDatabase.Refresh();
            Debug.Log("NotificationTypeNames.cs file created at: " + RootPath);
        }
    }
}