using UnityEditor;

namespace Snowy.Menu
{
    public class SnMenuCreator : EditorWindow
    {
        public MenuPrefabs menuPrefabs;
        
        public static void SetDefaultMenuPrefabs(MenuPrefabs menuPrefabs)
        {
            // Set the default menu prefabs for the script
            var window = GetWindow<SnMenuCreator>();
            window.menuPrefabs = menuPrefabs;
        }
    }
}