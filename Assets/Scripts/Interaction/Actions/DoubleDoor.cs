using System.Collections;
using UnityEngine;

namespace Interaction
{
    public class DoubleDoor : IInteractAction
    {
        public Interactable Interactable { get; set; }
        public string Prompt { get => "To Open"; set {} }
        
        [SerializeField] private Transform door1;
        [SerializeField] private Transform door2;
        [SerializeField] private float distance = 2f;
        [SerializeField] private float speed = 2f;
        
        private Vector3 door1StartPosition;
        private Vector3 door2StartPosition;
        
        private bool isOpen = false;
        
        public bool CanInteract { get; set; } = true;

        public void SetInteractable(Interactable interactable)
        {
            Interactable = interactable;
            
            door1StartPosition = door1.position;
            door2StartPosition = door2.position;
        }

        public void Interact(Interactor fpsInteractor)
        {
            if (isOpen)
            {
                CloseDoor();
            }
            else
            {
                OpenDoor();
            }
        }
        
        private void OpenDoor()
        {
            Interactable.StartCoroutine(MoveDoor(door1StartPosition + door1.right * distance, door1));
            Interactable.StartCoroutine(MoveDoor(door2StartPosition + door2.right * distance, door2));
            isOpen = true;
            CanInteract = false;
        }
        
        private void CloseDoor()
        {
            Interactable.StartCoroutine(MoveDoor(door1StartPosition, door1));
            Interactable.StartCoroutine(MoveDoor(door2StartPosition, door2));
            isOpen = false;
            CanInteract = false;
        }
        
        IEnumerator MoveDoor(Vector3 targetPosition, Transform door)
        {
            while (Vector3.Distance(door.transform.position, targetPosition) > 0.01f)
            {
                door.position = Vector3.MoveTowards(door.transform.position, targetPosition, speed * Time.deltaTime);
                yield return null;
            }
            
            CanInteract = true;
        }
    }
}