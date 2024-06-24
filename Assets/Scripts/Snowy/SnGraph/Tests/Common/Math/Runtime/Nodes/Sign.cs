
using UnityEngine;


namespace Snowy.SnGraph.Tests
{
    [Node(Path = "Math/Operation")]
    [Tags("Math")]
    public class Sign : MathNode<float, float>
    {
        public override float Execute(float value)
        {
            return Mathf.Sign(value);
        }
    }
}
