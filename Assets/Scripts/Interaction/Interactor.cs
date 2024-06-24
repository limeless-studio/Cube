using System.Linq;
using Snowy.FPS;
using Snowy.SnInput;
using UnityEngine;

namespace Interaction
{
    public class Interactor : MonoBehaviour
    {
        [SerializeField] bool canInteract = true;
        public Interactable interactable;
        private FPSCharacter character;

        public void OnSpawn(bool isOwner)
        {
            if (isOwner)
            {
                character = GetComponentInParent<FPSCharacter>();
                character.OnInputReceived += OnInputUpdated;
            } else
            {
                Destroy(this);
            }
        }

        private void OnInputUpdated(ref PlayerInputs input)
        {
            if (!canInteract || interactable == null) return;

            if ((interactable.action.IsPickup() && input.pickup == ButtonState.Pressed) ||
                (!interactable.action.IsPickup() && input.interact == ButtonState.Pressed))
            {
                interactable.Interact(this);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Interactable newInteractable))
            {
                CheckInteractable(newInteractable);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out Interactable newInteractable))
            {
                RemoveInteractable(newInteractable);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.TryGetComponent(out Interactable newInteractable))
            {
                CheckInteractable(newInteractable);
            }
        }

        private void CheckInteractable(Interactable newInteractable)
        {
            bool hasSight = Physics.Raycast(character.Camera.transform.position, character.Camera.transform.forward, out RaycastHit hit, 100f);
            Debug.Log(
                $"{hasSight} - {newInteractable.name} - {newInteractable.Colliders.Contains(hit.collider)} - {hit.collider.name}");

            if (!hasSight|| !newInteractable.Colliders.Contains(hit.collider))
            {
                if (newInteractable == interactable)
                {
                    RemoveInteractable(newInteractable);
                }

                return;
            }
            
            if (newInteractable.activateWithoutInput)
            {
                newInteractable.Interact(this);
            }
            else
            {
                if (interactable)
                {
                    float distance = Vector3.Distance(transform.position, newInteractable.transform.position);
                    float oldDistance = Vector3.Distance(transform.position, interactable.transform.position);
                    if (distance >= oldDistance) return;
                }

                // Remove the old notification
                if (newInteractable.CanInteract(this))
                {
                    if (interactable) RemoveInteractable(interactable);
                    interactable = newInteractable;
                }
            }
        }

        public void RemoveInteractable(Interactable inter)
        {
            if (interactable == inter)
            {
                interactable = null;
            }
        }
    }
}