using Snowy.SnGraph.Tests;

namespace Snowy.SnGraph.AI
{
    [Node(Path = "AI", Name = "Root", Deletable = false, Moveable = false)]
    [Tags("Hidden")]
    [Output("ExecOut", typeof(AINodeDataFlow), Multiple = true, Orientation = Orientation.Vertical)]
    public class AIRootNode : AINode
    {
        public AINode Child => Childs.Length > 0 ? Childs[0] : null;
        protected override NodeState OnRun() =>
            Child?.Run() ?? NodeState.Failure;
    }
}