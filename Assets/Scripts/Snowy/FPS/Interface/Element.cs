using Snowy.FPS;
using UnityEngine;

namespace Interface
{
    public class Element : MonoBehaviour
    {
        protected InterfaceManager interfaceManager;
        protected FPSCharacter character => interfaceManager.character;

        private void OnEnable()
        {
            if (interfaceManager == null) interfaceManager = GetComponentInParent<InterfaceManager>();
            if (interfaceManager == null) return;
            
            interfaceManager.RegisterElement(this);
            Enabled();
        }

        private void OnDisable()
        {
            if (interfaceManager) interfaceManager.UnRegisterElement(this);
        }

        public void Run()
        {
            Tick();
        }
        
        protected virtual void Enabled() {}

        protected virtual void Tick(){}
    }
}