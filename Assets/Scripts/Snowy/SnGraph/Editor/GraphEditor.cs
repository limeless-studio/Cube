using UnityEngine;
using UnityEditor;

namespace Snowy.SnGraph
{
    [CustomEditor(typeof(Graph), true)]
    public class GraphEditor : Editor
    {
        public GraphEditorWindow GetExistingEditorWindow()
        {
            var graph = target as Graph;

            var windows = Resources.FindObjectsOfTypeAll<GraphEditorWindow>();
            foreach (var window in windows)
            {
                if (window.Graph == graph)
                {
                    return window;
                }
            }

            return null;
        }
        
        public virtual GraphEditorWindow CreateEditorWindow()
        {
            var window = CreateInstance<GraphEditorWindow>();
            window.Show();
            window.Load(target as Graph);
            return window;
        }
        
        public GraphEditorWindow CreateOrFocusEditorWindow()
        {
            (target as Graph)?.OnGraphLoad();
            
            var window = GetExistingEditorWindow();
            if (!window)
            {
                window = CreateEditorWindow();
            }
            
            window.Focus();
            return window;
        }
    }
}