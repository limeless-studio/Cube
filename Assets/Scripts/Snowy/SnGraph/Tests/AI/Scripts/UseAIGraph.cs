using UnityEngine;
using UnityEngine.AI;

namespace Snowy.SnGraph.AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class UseAIGraph : MonoBehaviour
    {
        public AIGraph aiGraph;
        public NavMeshAgent agent;

        private AIGraph m_graph;
        
        private void OnEnable()
        {
            m_graph = Graph.Clone(aiGraph);
            m_graph.OnBehaviourEnable(this);
        }
        
        private void OnDisable()
        {
            m_graph.OnBehaviourDisable();
        }
        
        private void Start()
        {
            m_graph.OnBehaviourStart();
        }
        
        private void Update()
        {
            m_graph.OnBehaviourUpdate();
        }
    }
}