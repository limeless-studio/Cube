namespace Snowy.SnGraph.AI.Composite
{
    [Output("ExecOut", typeof(AINodeDataFlow), Multiple = true, Orientation = Orientation.Vertical)]
    public abstract class CompositeNode : AINode
    {
        [Input("ExecIn", Multiple = false, Orientation = Orientation.Vertical)] public AINodeDataFlow execIn;
    }
}