using UnityEngine;

namespace Snowy.SnGraph.Tests
{
    [CustomNodeView(typeof(Print))]
    class PrintNodeView : NodeView
    {
        protected override void OnInitialize()
        {
            Debug.Log("<b>[PrintNodeView]</b> OnInitialize");
        }

        public override void OnUpdate()
        {
            Debug.Log("<b>[PrintNodeView]</b> OnUpdate");
        }
    }
}
