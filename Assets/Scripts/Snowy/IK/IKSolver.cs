using Snowy.ActiveRagdoll.IK;
using UnityEngine;

namespace Snowy.IK
{
    [ExecuteAlways]
    public class IKSolver : MonoBehaviour
    {
        [SerializeField] IKComponent[] ikComponents;
        [SerializeField] bool editMode = false;
        
        void Start()
        {
            FetchIKComponents();
        }

        public void FetchIKComponents()
        {
            ikComponents = GetComponentsInChildren<IKComponent>(false);
            foreach (var ikComponent in ikComponents)
            {
                ikComponent.Init(this);
            }
        }

        private void LateUpdate()
        {
            # if UNITY_EDITOR
            if (!editMode && !Application.isPlaying) return;
            foreach (var ikComponent in ikComponents)
            {
                ikComponent.Solve();
            }
            #else
            foreach (var ikComponent in ikComponents)
            {
                ikComponent.Solve();
            }
            #endif
        }
    }
}