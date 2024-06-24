using UnityEngine;

namespace Interaction
{
    public interface IInteractAction
    {
        Interactable Interactable { get; set; }
        public string Prompt { get; set; }
        public bool CanInteract { get; set; }

        public virtual void SetInteractable(Interactable interactable)
        {
            Interactable = interactable;
        }

        void Interact(Interactor fpsInteractor);
        
        public bool IsPickup() => false;
    }
}