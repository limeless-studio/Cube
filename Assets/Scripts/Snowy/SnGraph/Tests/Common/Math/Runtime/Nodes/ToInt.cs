namespace Snowy.SnGraph.Tests
{
    
    [Node( Name= "ToInt", Path = "Math/Utils/ToInt")]
    [Tags("Math")]
    public class ToInt : MathNode<float, int>
    {
        public override int Execute(float value)
        {
            return (int)value;
        }
    }
}