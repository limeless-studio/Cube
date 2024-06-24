using Snowy.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Snowy.Menu
{
    [CreateAssetMenu(fileName = "MenuPrefabs", menuName = "Snowy/UI/Menu/Menu Prefabs")]
    public class MenuPrefabs : ScriptableObject
    {
        // ButtonPrefab, menuPrefab, TextPrefab, etc.
        public SnMenuManager menuManager;
        public SnButton buttonPrefab;
        public SnMenu menuPrefab;
        public HorizontalOrVerticalLayoutGroup buttonLayoutGroup;
        public HorizontalOrVerticalLayoutGroup titleLayoutGroup;
        
        [EditorButton("SetDefaultMenuPrefabs")]
        [SerializeField, Hide] bool isDefaultMenuPrefabsSet;
        
        # if UNITY_EDITOR
        
        public void SetDefaultMenuPrefabs()
        {
            SnMenuManager.SetDefaultMenuPrefabs(this);
        }
        # endif
    }
}