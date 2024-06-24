using Snowy.SnGraph.Tests;
using UnityEngine;
using UnityEngine.UIElements;

namespace Snowy.SnGraph.AI.Editor
{
    [CustomNodeView(typeof(AINode))]
    public class AINodeView : NodeView
    {
        protected override void OnInitialize()
        {
            styleSheets.Add(Resources.Load<StyleSheet>("SnGraphSamples/AINodeView"));
            AddToClassList("aiNodeView");
            
            // Vertical layout
            // Create a input container on the top before the title section
            var nodeBorder = this.Q<VisualElement>("node-border");
            
            // Top input container clone inputContainer
            var tContainer = new VisualElement();
            tContainer.name = "topInputContainer";
            foreach (var c in inputContainer.GetClasses())
            {
                tContainer.AddToClassList(c);
            }
            
            // Add the input to the input container
            PortView inView = GetInputPort("ExecIn");

            if (inView != null)
            {
                inView.AddToClassList("execInPortView");
                // Make inView the first
                tContainer.Add(inView);
            }
            
            // Add the input container before the title
            nodeBorder.Insert(0, tContainer);
            
            // Add the output to the output container
            PortView outView = GetOutputPort("ExecOut");
            
            if (outView != null)
            {
                outView.AddToClassList("execOutPortView");
                
                // Make outView the last
                outputContainer.Add(outView);
            }
            
            // Add the output container after the title
            nodeBorder.Add(outputContainer);
        }
    }
}