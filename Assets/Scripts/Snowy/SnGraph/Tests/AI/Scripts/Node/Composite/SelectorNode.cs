namespace Snowy.SnGraph.AI.Composite
{
    [Node(Path = "Composite", Name = "Selector")]
    [Tags("AI")]
    public class SelectorNode : CompositeNode
    {
        protected override NodeState OnRun()
        {
            foreach (var child in Childs)
            {
                switch (child.Run())
                {
                    case NodeState.Running:
                        return NodeState.Running;
                    case NodeState.Success:
                        return NodeState.Success;
                    case NodeState.Failure:
                        continue;
                }
            }
            
            return NodeState.Failure;
        }
    }
}