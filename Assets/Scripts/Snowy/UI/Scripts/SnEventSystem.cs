using Snowy.SnInput;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace Menu
{
    public class SnEventSystem : EventSystem
    {
        [SerializeField] private bool dontDestroyOnLoad;
        InputAction cancelAction;
        
        protected override void Awake()
        {
            base.Awake();
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        
        protected override void Start()
        {
            base.Start();

            // Set the current control scheme to the gamepad
            InputManager.Instance.ListenToSchemeChange((scheme) =>
            {
                if (scheme == "Gamepad")
                {
                    if (!EventSystem.current) return;
                    if (!EventSystem.current.currentSelectedGameObject)
                    {
                        // If there is a menu manager script in the project
# if MenuManager
                        if (MenuManager.Instance)
                        {
                            if (MenuManager.Instance.CurrentMenu)
                            {
                                MenuManager.Instance.CurrentMenu.FocusOnDefault();
                                return;
                            }
                        }
# endif
                        
                        EventSystem.current.SetSelectedGameObject(EventSystem.current.firstSelectedGameObject);
                    }
                    
                    // Lock the cursor
                    Cursor.visible = false;
                }
                else
                {
                    // Unlock the cursor
                    Cursor.visible = true;
                }
            });
            
            var module = EventSystem.current.GetComponent<InputSystemUIInputModule>();
            if (module)
            {
                cancelAction = module.cancel.action;
            }
        }

        protected override void Update()
        {
            base.Update();
            // Check if the current active input is the gamepad
           if (InputManager.Instance.GetCurrentControlScheme() == "Gamepad")
           {
               // If nothing is selected, select the first button
               if (!current.currentSelectedGameObject)
               {
                   current.SetSelectedGameObject(current.firstSelectedGameObject);
               }
               
               Cursor.visible = false;
           } 
           else
           {
               Cursor.visible = true;
           }
           
           if (cancelAction != null && cancelAction.triggered)
           {
               // if was pressed rn
# if MenuManager
               if (MenuManager.Instance)
               {
                   if (MenuManager.Instance.CurrentMenu)
                   {
                       MenuManager.Instance.CurrentMenu.Back();
                   }
               }
#endif
           }
            
        }
    }
}