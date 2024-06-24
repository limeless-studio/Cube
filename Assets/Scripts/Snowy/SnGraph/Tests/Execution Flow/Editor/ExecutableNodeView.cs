
using UnityEngine;
using UnityEngine.UIElements;



namespace Snowy.SnGraph.Tests
{
    [CustomNodeView(typeof(ExecutableNode))]
    [CustomNodeView(typeof(EventNode))]
    public class ExecutableNodeView : NodeView
    {
        protected override void OnInitialize()
        {
            styleSheets.Add(Resources.Load<StyleSheet>("SnGraphSamples/ExecutableNodeView"));
            AddToClassList("executableNodeView");

            if (Target is EventNode)
            {
                AddToClassList("eventNodeView");
            }

            // Customize placement of the default exec IO ports 
            PortView inView = GetInputPort("ExecIn");
            PortView outView = GetOutputPort("ExecOut");

            if (inView != null) inView.AddToClassList("execInPortView");
            
            if (outView != null) outView.AddToClassList("execOutPortView");
        }
    }
}
