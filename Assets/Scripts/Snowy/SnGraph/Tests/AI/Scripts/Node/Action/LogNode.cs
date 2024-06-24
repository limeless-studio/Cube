using UnityEngine;

namespace Snowy.SnGraph.AI.Action
{
    public enum LogMode
    {
        Debug,
        Warning,
        Error
    };
    
    [Node(Path = "Action")]
    [Tags("AI")]
    public class LogNode : TaskNode
    {
        [Input("ExecIn", Multiple = false, Orientation = Orientation.Vertical)] public AINodeDataFlow execIn;

        [Input] public string message;
        [Input(Editable = false)] public object obj;
        [Input] public Object context;

        [Editable] public LogMode mode;

        protected override NodeState OnRun()
        {
            string msg = GetInputValue("message", message);
            object obj = GetInputValue("obj", this.obj);
            Object context = GetInputValue("context", this.context);

            if (obj != null)
            {
                msg += obj.ToString();
            }
            
            switch (mode)
            { 
                case LogMode.Debug:
                    Debug.Log(msg, context);
                    break;
                case LogMode.Warning:
                    Debug.LogWarning(msg, context);
                    break;
                case LogMode.Error:
                    Debug.LogError(msg, context);
                    break;
                default: break;
            }
            
            return NodeState.Success;
        }
    }
}