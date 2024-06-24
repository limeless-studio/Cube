using System;
using System.Collections;
using Snowy.Utils;
using Unity.Netcode;
using UnityEngine;

namespace Interaction
{
    public class Interactable : MonoBehaviour, IInteractable
    {
        [SerializeField] public bool isActive = true;
        public bool activateWithoutInput = false;
        [SerializeField] bool onlyHost = true;
        [SerializeField] private Collider[] colliders;
        [InteractActions] public string actionType;
        [SerializeReference] public IInteractAction action;
        private Collider m_collider;

        public Collider[] Colliders => colliders;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (actionType == null || actionType == "") return;

            Type type = Utilities.GetType(actionType);

            if (type == null)
            {
                Debug.LogError("Type " + actionType + " does not exist!");
                return;
            }

            if (type.GetInterface("IInteractAction") == null)
            {
                Debug.LogError("Type " + actionType + " does not implement IInteractAction!");
            }
            else if (action == null || action.GetType() != type)
            {
                // Create it as a subclass of IInteractAction
                action = (IInteractAction)Activator.CreateInstance(type);
                // change the action type to the type name
                action.SetInteractable(this);
            }
            else
            {
                if (action != null) action.SetInteractable(this);
                else
                {
                    Debug.LogError("Action is null!");
                }
            }
        }
#endif

        IEnumerator Start()
        {
            if (!TryGetComponent(out m_collider))
            {
                Debug.LogError("Interactable object " + gameObject.name +
                               " does not have a collider!\nAdding a BoxCollider by default.");
                m_collider = gameObject.AddComponent<BoxCollider>();
            }

            // cHECK IF THERE IS MORE THAN ONE COLLIDER
            var colliders = GetComponents<Collider>();
            if (colliders.Length > 1)
            {
                // Get the one with trigger on
                foreach (var col in colliders)
                {
                    if (col.isTrigger)
                    {
                        m_collider = col;
                        break;
                    }
                }

                // If none of them are triggers, get the first one
                if (!m_collider.isTrigger)
                {
                    m_collider = colliders[0];
                }
            }

            if (m_collider.GetType() != typeof(MeshCollider))
            {
                m_collider.isTrigger = true;
            }

            action.SetInteractable(this);

            m_collider.enabled = false;
            yield return new WaitForSeconds(0.1f);
            m_collider.enabled = true;
        }

        public void Interact(Interactor fpsInteractor)
        {
            action.Interact(fpsInteractor);
        }

        public bool CanInteract(Interactor fpsInteractor)
        {
            if (!isActive) return false;
            if (onlyHost && !NetworkManager.Singleton.IsHost) return false;
            var forward = fpsInteractor.transform.forward;
            var toInteractable = transform.position - fpsInteractor.transform.position;
            forward.y = 0;
            toInteractable.y = 0;
            if (Vector3.Angle(forward, toInteractable) > 45) return false;
            return action.CanInteract;
        }

        public bool IsActive() => isActive;
    }
}