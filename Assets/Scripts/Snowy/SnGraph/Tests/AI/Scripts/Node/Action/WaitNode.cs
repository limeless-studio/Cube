namespace Snowy.SnGraph.AI.Action
{
    [Node(Path = "Action")]
    [Tags("AI")]
    public class WaitNode : TaskNode
    {        [Input("ExecIn", Multiple = false, Orientation = Orientation.Vertical)] public AINodeDataFlow execIn;

        [Input(Editable = true)] public float seconds;

        public float StartTime { get; private set; }

        protected override void OnNodeStart()
        {
            StartTime = UnityEngine.Time.time;
            base.OnNodeStart();
        }

        protected override NodeState OnRun()
        {
            if (IsTimeOver())
            {
                return NodeState.Success;
            }
            
            return NodeState.Running;
        }

        public bool IsTimeOver() => UnityEngine.Time.time - StartTime > seconds;
    }
}