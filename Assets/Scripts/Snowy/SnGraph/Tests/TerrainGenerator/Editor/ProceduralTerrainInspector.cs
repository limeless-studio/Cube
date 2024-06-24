using Snowy.SnGraph;
using Snowy.SnGraph.Tests;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProceduralTerrain))]
public class ProceduralTerrainInspector : GraphEditor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Apply to Terrain"))
        {
            (target as ProceduralTerrain).Execute();
        }

        base.OnInspectorGUI();
    }
}