using UnityEngine;
using UnityEngine.AI;

namespace Snowy.SnGraph.AI
{
    public enum NodeState
    {
        Running,
        Failure,
        Success
    } 
    
    [CreateAssetMenu(menuName = "SnGraph/AI/AIGraph")]
    [IncludeTags("AI")]
    public class AIGraph : Graph
    {
        public UseAIGraph UseAIGraph { get; set; }
        public NavMeshAgent NavMeshAgent { get; set; }
        public Transform Transform { get; set; }
        
        public AIRootNode RootNode { get; set; }

        public override float ZoomMaxScale { get; } = 2f;
        public override float ZoomMinScale { get; } = .7f;

        # if UNITY_EDITOR
        public override void OnGraphLoad()
        {
            RootNode = GetNode<AIRootNode>();
            if (RootNode == null)
            {
                AddNode(NodeReflection.Instantiate<AIRootNode>());
                
                RootNode = GetNode<AIRootNode>();
            }
        }
        #endif

        #region Mono Events
        public void OnBehaviourEnable(UseAIGraph graph)
        {
            UseAIGraph = graph;
            NavMeshAgent = UseAIGraph.agent;
            Transform = UseAIGraph.transform;
            
            if (RootNode == null) RootNode = GetNode<AIRootNode>();
        }
        
        public void OnBehaviourDisable()
        {
        }
        
        public void OnBehaviourStart()
        {
        }
        
        
        public void OnBehaviourUpdate()
        {
            // RootNode.Run();
            if (RootNode == null)
            {
                Debug.LogError("RootNode is null");
                return;
            }
            RootNode.Run();
        }
        
        #endregion
        
        void Traverse()
        {
            
        }
    }
}