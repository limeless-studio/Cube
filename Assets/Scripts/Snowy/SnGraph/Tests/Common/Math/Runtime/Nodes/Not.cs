
using UnityEngine;


namespace Snowy.SnGraph.Tests
{
    [Node(Path = "Math/Boolean")]
    [Tags("Math")]
    public class Not : MathNode<bool, bool>
    {
        public override bool Execute(bool value)
        {
            return !value;
        }
    }
}
