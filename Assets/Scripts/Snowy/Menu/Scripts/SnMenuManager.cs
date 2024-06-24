using System.Collections;
using System.Collections.Generic;
using Snowy.Utils;
using UnityEngine;

namespace Snowy.Menu
{
    public class SnMenuManager : MonoSingleton<SnMenuManager>
    {
        public static MenuPrefabs menuPrefabs;
        [SerializeField] List<SnMenu> m_menus;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float cameraDuration = 1f;
        [SerializeField] private bool fetchOnAwake = true;
        [SerializeField] private int defaultMenuID = -1;
        [SerializeField] private bool debug;
        
        private SnMenu m_currentMenu;
        
        # region Unity Methods
        
        protected override void Awake()
        {
            base.Awake();
            if (fetchOnAwake)
                FetchMenus();

            if (defaultMenuID != -1)
            {
                var menu = m_menus.Find(m => m.MenuID == defaultMenuID);
                menu.OpenMenu(true);
            }
        }
        
        # endregion
        
        # region Public API Methods
        
        
        IEnumerator MoveCameraCoroutine(Transform target, int menuID)
        {
            var startPosition = cameraTransform.position;
            var targetPosition = target.position;
            var startRotation = cameraTransform.rotation.eulerAngles;
            var targetRotation = target.rotation.eulerAngles;
            var time = 0f;
            
            while (time < cameraDuration)
            {
                time += Time.deltaTime / cameraDuration;
                cameraTransform.position = Vector3.Slerp(startPosition, targetPosition, Mathf.SmoothStep(0f, 1f, time));
                cameraTransform.rotation = Quaternion.Euler(Vector3.Slerp(startRotation, targetRotation, Mathf.SmoothStep(0f, 1f, time)));
                yield return null;
            }
            
            OpenMenu(menuID);
        }
        
        public void OpenMenu(int menuID, Transform target, bool openFirst)
        {
            CloseAllMenus();
            if (openFirst)
                OpenMenu(menuID);
            StartCoroutine(MoveCameraCoroutine(target, menuID));
        }

        /// <summary>
        /// Open a menu by ID and close all other menus
        /// </summary>
        /// <param name="menuID">int</param>
        public void OpenMenu(int menuID)
        {
            var menu = m_menus.Find(m => m.MenuID == menuID);
            if (menu != null)
            {
                // Close any other open menus
                CloseAllMenus();
            
                if (menu.CanSavePreviousMenu) 
                    menu.SetPreviousMenuID(m_currentMenu?.MenuID ?? -1);
                
                menu.Open();
                m_currentMenu = menu;
            }
            else
            {
                Debug.LogWarning($"Menu with ID {menuID} not found");
            }
        }
        
        /// <summary>
        /// Close all menus, if they are open
        /// </summary>
        public void CloseAllMenus()
        {
            foreach (var menu in m_menus)
                menu.Close();
            
            m_currentMenu = null;
        }
        
        # endregion
        
        # region Generation and Management

        private void ValidateMenus()
        {
            if (m_menus == null)
                m_menus = new List<SnMenu>();
            for (int i = 0; i < m_menus.Count; i++)
            {
                if (m_menus[i] == null)
                {
                    m_menus.RemoveAt(i);
                    i--;
                }
            }
            
            // Default menu ID must be valid
            if (defaultMenuID != -1 && !m_menus.Exists(m => m.MenuID == defaultMenuID))
                defaultMenuID = -1;
        }
        
        private SnMenu[] GetMenus()
        {
            ValidateMenus();
            return m_menus.ToArray();
        }
        
        public List<SnMenu> GetMenusList()
        {
            ValidateMenus();
            return m_menus;
        }
        
        /// <summary>
        /// Add all menus in children to the list, if they are not already in the list
        /// </summary>
        public void FetchMenus()
        {
            var menus = GetComponentsInChildren<SnMenu>(true);
            foreach (var menu in menus)
                AddMenu(menu);
        }
        
        /// <summary>
        /// Generate a unique ID for the menu
        /// </summary>
        /// <returns>integer</returns>
        private int GenerateId()
        {
            int id = 0;
            while (m_menus.Exists(m => m.MenuID == id))
                id++;
            return id;
        }
        
        /// <summary>
        /// Add a menu to the list, if it is not already in the list
        /// </summary>
        /// <param name="menu">SnMenu</param>
        public void AddMenu(SnMenu menu)
        {
            if (!m_menus.Contains(menu))
            {
                int id = GenerateId();
                menu.SetMenuID(id);
                m_menus.Add(menu);
            }
        }
        
        /// <summary>
        /// Remove a menu from the list
        /// </summary>
        /// <param name="menuMenuID">int</param>
        public void RemoveMenu(int menuMenuID)
        {
            var menu = m_menus.Find(m => m.MenuID == menuMenuID);
            if (menu != null)
            {
                m_menus.Remove(menu);
                # if UNITY_EDITOR
                if (Application.isPlaying)
                    Destroy(menu.gameObject);
                else
                    DestroyImmediate(menu.gameObject);
                # else
                Destroy(menu.gameObject);
                # endif
            }
        }
        
        public void RegenerateMenuIDs()
        {
            for (int i = 0; i < m_menus.Count; i++)
                m_menus[i].SetMenuID(i);
        }
        
        # endregion
        
        public static void SetDefaultMenuPrefabs(MenuPrefabs prefabs)
        {
            menuPrefabs = prefabs;
            Debug.Log("Default menu prefabs set");
        }

        public void GoBack()
        {
            if (m_currentMenu != null && m_currentMenu.CanSavePreviousMenu)
            {
                var prevMenu = m_currentMenu;
                var nextMenuID = prevMenu.GetPreviousMenuID();
                var nextMenu = m_menus.Find(m => m.MenuID == nextMenuID);
                nextMenu.OpenMenu(true);
                prevMenu.ClearPreviousMenuID();
            }
        }
    }
}