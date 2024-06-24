namespace Snowy.SnGraph.AI.Composite
{
    [Node(Path = "Composite", Name = "Sequencer")]
    [Tags("AI")]
    public class SequencerNode : CompositeNode
    {
        [Editable] public bool UpdateInTheSameFrame;
        private int m_current;

        protected override void OnNodeStart()
        {
            m_current = 0;
            base.OnNodeStart();
        }

        protected override NodeState OnRun()
        {
            if (Childs.Length < 0) return NodeState.Success;

            do
            {
                if (m_current >= Childs.Length) return NodeState.Success;
                
                var child = Childs[m_current];
                if (child == null)
                {
                    m_current++;
                    continue;
                }
                
                switch (child.Run())
                {
                    case NodeState.Running:
                        return NodeState.Running;
                    case NodeState.Success:
                        m_current++;
                        break;
                    case NodeState.Failure:
                        return NodeState.Failure;
                }
            } while (m_current < Childs.Length && UpdateInTheSameFrame);
            
            return m_current >= Childs.Length ? NodeState.Success : NodeState.Running;
        }
    }
}