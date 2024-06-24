using Snowy.SnGraph.Tests;
using UnityEngine;
using UnityEngine.UIElements;

namespace Snowy.SnGraph.Tests
{
    /// <summary>
    /// Base view for math nodes
    /// </summary>
    [CustomNodeView(typeof(MathNode))]
    public class MathNodeView : NodeView
    {
        protected override void OnInitialize()
        {
            base.OnInitialize();

            styleSheets.Add(Resources.Load<StyleSheet>("SnGraphSamples/MathNodeView"));
            AddToClassList("mathNodeView");
        }
    }
}
