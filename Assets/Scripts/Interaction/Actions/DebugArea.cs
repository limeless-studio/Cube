using UnityEngine;

namespace Interaction
{
    public class DebugArea : IInteractAction
    {
        public string debugText = "This is a debug text";
        public Interactable Interactable { get; set; }
        public string Prompt { get => "To interact"; set {} }
        
        public bool CanInteract { get; set; } = true;

        public void Interact(Interactor fpsInteractor)
        {
            Debug.Log(debugText);
        }
    }
}