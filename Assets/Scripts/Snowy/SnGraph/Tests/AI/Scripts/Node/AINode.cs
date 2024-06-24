using System;
using System.Linq;
using Snowy.SnGraph.Tests;
using UnityEngine;
using UnityEngine.AI;

namespace Snowy.SnGraph.AI
{
    public class AINodeDataFlow { }
    
    [Node(Path = "AI")]
    [Tags("AI")]
    //[Output("ExecOut", typeof(ExecutionFlowData), Multiple = true, Orientation = Orientation.Vertical)]
    public abstract class AINode : Node
    {
        public AIGraph AIGraph => (AIGraph)Graph;
        public UseAIGraph UseAIGraph => AIGraph.UseAIGraph;
        public NavMeshAgent Agent => AIGraph.NavMeshAgent;
        public Transform Transform => AIGraph.Transform;

        protected AINode[] Childs = Array.Empty<AINode>();
        
        public NodeState State { get; set; } 
        private bool m_started;
        //[Input("ExecIn", Multiple = false, Orientation = Orientation.Vertical)] public ExecutionFlowData execIn;

        public override object OnRequestValue(Port port) => null;
        
        protected virtual void OnNodeEnter() { }
        
        protected virtual void OnNodeExit() { }
        protected virtual void OnNodeStop() {}

        protected virtual void OnNodeStart()
        {
            State = NodeState.Running;
            Childs = GenerateChildNodes();
        }
        
        public NodeState Run()
        {
            OnNodeEnter();
            if (!m_started)
            {
                OnNodeStart();
                m_started = true;
            }
            
            State = OnRun();
            
            if (State != NodeState.Running)
            {
                OnNodeExit();
                m_started = false;
            }
            
            return State;
        }
        
        protected abstract NodeState OnRun();
        

        protected AINode[] GenerateChildNodes(string portName = "ExecOut")
        {
            var port = GetPort(portName);
            if (port == null || port.ConnectionCount < 1) 
            {
                return Array.Empty<AINode>();
            }
            
            return port.ConnectedPorts.Select(p => p.Node as AINode).ToArray();
        }
        
        protected AINode[] GetChilds()
        {
            return Childs;
        }
    }
}