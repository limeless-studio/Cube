using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Snowy.SnGraph
{
    public class GraphEditorWindow : EditorWindow
    {
        public CanvasView Canvas { get; protected set; }

        public Graph Graph { get; protected set; }
        
        public virtual void Load(Graph graph)
        {
            Graph = graph;

            Canvas = new CanvasView(this);
            Canvas.Load(graph);
            Canvas.StretchToParentSize();
            rootVisualElement.Add(Canvas);

            titleContent = new GUIContent(graph.name);
            Repaint();
        }

        protected virtual void Update()
        {
            if (Canvas == null)
            {
                Close();
                return;
            }

            Canvas.Update();
        }
        
        protected virtual void OnEnable()
        {
            if (Graph)
            {
                Load(Graph);
            }
        }
    }
}