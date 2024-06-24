using UnityEngine;
using UnityEngine.UI;

namespace Snowy.Menu
{
    public class SnMenu : MonoBehaviour
    {
        [SerializeField] HorizontalOrVerticalLayoutGroup buttonLayoutGroup;
        [SerializeField] HorizontalOrVerticalLayoutGroup titleLayoutGroup;
        [SerializeField] bool canSavePreviousMenu = true;
        [SerializeField] bool moveCameraToMenu;
        [SerializeField] private Transform camPosition;
        
        [SerializeField] int m_previousMenuID = -1;
        [SerializeField] int m_menuID = -1;
        
        public string MenuName => gameObject.name;
        public bool CanSavePreviousMenu => canSavePreviousMenu;
        public int MenuID => m_menuID;

        /// <summary>
        /// Open the menu
        /// </summary>
        public void Open()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Close the menu
        /// </summary>
        public void Close()
        {
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Set the menu ID
        /// </summary>
        /// <param name="menuID">int</param>
        public void SetMenuID(int menuID)
        {
            m_menuID = menuID;
        }
        
        /// <summary>
        /// Set the previous menu ID
        /// </summary>
        /// <param name="menuID">int</param>
        public void SetPreviousMenuID(int menuID)
        {
            m_previousMenuID = menuID;
        }
        
        /// <summary>
        /// Gets the previous menu ID
        /// </summary>
        /// <returns>integer</returns>
        public int GetPreviousMenuID()
        {
            return m_previousMenuID;
        }
        
        /// <summary>
        /// Clear the previous menu ID, set to -1
        /// </summary>
        public void ClearPreviousMenuID()
        {
            m_previousMenuID = -1;
        }

        public void OpenMenu(bool openFirst = false)
        {
            if (moveCameraToMenu)
            {
                SnMenuManager.Instance.OpenMenu(m_menuID, camPosition, openFirst);
            }
            else
            {
                SnMenuManager.Instance.OpenMenu(m_menuID);
            }
        }

        public void OpenMenu()
        {
            OpenMenu(false);
        }
    }
}