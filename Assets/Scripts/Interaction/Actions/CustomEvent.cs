using UnityEngine.Events;

namespace Interaction
{
    public class CustomEvent : IInteractAction 
    {
        public UnityEvent customEvent;
        public Interactable Interactable { get; set; }

        public string Prompt
        {
            get => "To interact";
            set { }
        }

        public bool CanInteract { get; set; } = true;

        public void Interact(Interactor interactor)
        {
            customEvent.Invoke();
        }
    }
}