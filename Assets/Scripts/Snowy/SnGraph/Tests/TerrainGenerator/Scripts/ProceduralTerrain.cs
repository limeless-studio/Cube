using System;
using UnityEngine;

namespace Snowy.SnGraph.Tests
{
    [RequireComponent(typeof(Terrain))]
    public class ProceduralTerrain : MonoBehaviour
    {
        
        public TerrainGraph graph;
        
        private void Start()
        {
            Execute();
        }

        public void Execute()
        {
            if (!graph)
            {
                Debug.LogError("No graph assigned to the procedural terrain");
                return;
            }
            
            var terrain = GetComponent<Terrain>();
            var data = terrain.terrainData;
            
            var resolution = data.heightmapResolution;
            var fres = (float)resolution;
            
            # if UNITY_EDITOR
            UnityEditor.EditorUtility.DisplayProgressBar("Generate Hightmap", "Evaluate graph", 0);
            # endif
            
            var heightmap = graph.GetOutputHeightmap();
            if (heightmap == null)
            {
                Debug.LogError("No heightmap output found in the graph");
                # if UNITY_EDITOR
                UnityEditor.EditorUtility.ClearProgressBar();
                # endif
                return;
            }
            
            var heights = data.GetHeights(0, 0, resolution, resolution);

            for (int y = 0; y < resolution; y++)
            {
#if UNITY_EDITOR
                if (y % 20 == 0)
                {
                    UnityEditor.EditorUtility.DisplayProgressBar(
                        "Generate Heightmap", 
                        "Creating heightmap texture", 
                        Mathf.InverseLerp(0.0f, fres, y)
                    );
                }
#endif

                for (int x = 0; x < resolution; x++)
                {
                    heights[y, x] = heightmap.GetHeightBilinear(x / fres, y / fres);
                }
            }
            
#if UNITY_EDITOR
            UnityEditor.EditorUtility.ClearProgressBar();
#endif 

            data.SetHeights(0, 0, heights);
        }
    }
}