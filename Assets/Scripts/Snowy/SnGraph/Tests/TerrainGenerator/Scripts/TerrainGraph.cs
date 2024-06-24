using System;
using UnityEngine;

namespace Snowy.SnGraph.Tests
{
    [CreateAssetMenu(
        fileName = "TerrainGraph",
        menuName = "Snowy/SnGraph/TerrainGenerator/TerrainGraph"
    )]
    
    [IncludeTags("Heightmap")]
    public class TerrainGraph : Graph
    {
# if UNITY_EDITOR
        public override void OnGraphLoad()
        {
            if (GetNode<TerrainOutput>() == null)
            {
                AddNode(NodeReflection.Instantiate<TerrainOutput>());
            }
        }
#endif
        
        public Heightmap GetOutputHeightmap()
        {
            var output = GetNode<TerrainOutput>();
            var heightmap = output?.GetInputValue<Heightmap>("map");
            return heightmap;
        }
    }
}