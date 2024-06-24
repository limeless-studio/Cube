using System;
using System.Collections.Generic;
using System.IO;
using Snowy.Utils;
using UnityEditor;
using UnityEngine;

public class SnMeshCombiner : EditorWindow
{
    private DefaultAsset m_exportDirectory;
    private GameObject m_targetObject;
    private bool m_exportMesh;
    
    [MenuItem("Snowy/SnMeshCombiner")]
    static void Init()
    {
        SnMeshCombiner window = (SnMeshCombiner)GetWindow(typeof(SnMeshCombiner));
        window.Show();
    }

    private void OnGUI()
    {
        SnEditorGUI.DrawTitle("Mesh Combiner");
        m_targetObject = EditorGUILayout.ObjectField("Target Object", m_targetObject, typeof(GameObject), true) as GameObject;
        m_exportMesh = EditorGUILayout.Toggle("Export Mesh", m_exportMesh);
        
        // With button to choose directory
        EditorGUILayout.BeginHorizontal();
        m_exportDirectory = EditorGUILayout.ObjectField("Export Directory", m_exportDirectory, typeof(DefaultAsset), false) as DefaultAsset;
        
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("Combine"))
        {
            if (m_targetObject == null)
            {
                Debug.LogError("Target Object is null");
                return;
            }

            if (m_exportMesh && m_exportDirectory == null)
            {
                Debug.LogError("Export Directory is null");
                return;
            }

            CombineMesh();
        }
    }
    
    void CombineMesh()
    {
        MeshFilter[] meshFilters = m_targetObject.GetComponentsInChildren<MeshFilter>();
        var combineMeshInstance = new Dictionary<Material, List<CombineInstance>>();
        
        Debug.Log("Mesh filters: " + meshFilters.Length);

        foreach (var meshFilter in meshFilters)
        {
            var mesh = meshFilter.sharedMesh;
            List<Vector3> vertices = new List<Vector3>();
            var materials = meshFilter.GetComponent<Renderer>().sharedMaterials;
            var subMeshCount = meshFilter.sharedMesh.subMeshCount;
            mesh.GetVertices(vertices);

            for (int i = 0; i < subMeshCount; i++)
            {
                var material = materials[i];
                var triangles = new List<int>();
                mesh.GetTriangles(triangles, i);

                var newMesh = new Mesh
                {
                    vertices = vertices.ToArray(),
                    triangles = triangles.ToArray(),
                    uv = mesh.uv,
                    normals = mesh.normals,
                };

                if (!combineMeshInstance.ContainsKey(material))
                {
                    combineMeshInstance.Add(material, new List<CombineInstance>());
                }
                
                var combineInstance = new CombineInstance
                {
                    transform = meshFilter.transform.localToWorldMatrix,
                    mesh = newMesh,
                };
                
                combineMeshInstance[material].Add(combineInstance);
            }
        }
        
        Debug.Log("Combining meshes: " + combineMeshInstance.Count);
        m_targetObject.SetActive(false);
        
        foreach (var kvp in combineMeshInstance)
        {
            var newObject = new GameObject(kvp.Key.name);
            
            var meshRenderer = newObject.AddComponent<MeshRenderer>();
            var meshFilter = newObject.AddComponent<MeshFilter>();
            
            meshRenderer.sharedMaterial = kvp.Key;
            var newMesh = new Mesh();
            newMesh.CombineMeshes(kvp.Value.ToArray());
            Unwrapping.GenerateSecondaryUVSet(newMesh);
            
            meshFilter.sharedMesh = newMesh;
            newObject.transform.SetParent(m_targetObject.transform.parent);
            
            if (!m_exportMesh || m_exportDirectory == null) continue;
            
            ExportMesh(newMesh, kvp.Key.name);
        }
    }

    void ExportMesh(Mesh mesh, string fileName)
    {
        var exportDirPath = AssetDatabase.GetAssetPath(m_exportDirectory);
        if (Path.GetExtension(fileName) != ".asset")
        {
            fileName += ".asset";
        }
        
        Debug.Log("Exporting mesh to: " + exportDirPath);
        
        var exportPath = Path.Combine(exportDirPath, fileName);
        AssetDatabase.CreateAsset(mesh, exportPath);
    }
}
