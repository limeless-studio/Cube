using System.Linq;
using System.Reflection;
using Snowy.Utils;
using Toolbox.Editor;
using Toolbox.Editor.Internal;
using UnityEditor;
using UnityEngine;

namespace Snowy.FPS.Editor
{
    [CustomEditor(typeof(FPSMovement))]
    public class FPSMovementEditor : ToolboxEditor
    {
        ReorderableList list;
        
        protected void OnEnable()
        {
            list = new ReorderableList(serializedObject.FindProperty("components"), "Components", true, true, true, false);
            
            // Edit the size of the element
            list.elementHeightCallback = (index) =>
            {
                try
                {
                    var component = (target as FPSMovement).GetComponentAtIndex(index);
                
                    if (component == null)
                    {
                        return EditorGUIUtility.singleLineHeight;
                    }
                
                    var fields = GetFields(component);
                    var height = EditorGUIUtility.singleLineHeight;
                    var prop = list.List.GetArrayElementAtIndex(index);
                
                    if (!prop.isExpanded)
                    {
                        return height;
                    }
                
                    foreach (var field in fields)
                    {
                        var fieldProp = prop.FindPropertyRelative(field.Name);
                        height += EditorGUI.GetPropertyHeight(fieldProp) + EditorGUIUtility.standardVerticalSpacing;
                    }
                
                    height += EditorGUIUtility.standardVerticalSpacing;
                    return height;
                } catch
                {
                    return EditorGUIUtility.singleLineHeight;
                }
            };
            
            // TODO: MOVEMENT COMPONENT PROPERTY DRAWER.
            // Draw the background of the element
            list.drawElementBackgroundCallback = (rect, index, isActive, isFocused) =>
            {
                // Draw a background color
                if (isActive || isFocused)
                {
                    UnityEditor.EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f, 0.5f));
                }
            };
            
            // Draw the element
            list.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                // Draw the element
                var component = (target as FPSMovement).GetComponentAtIndex(index);
                var prop = list.List.GetArrayElementAtIndex(index);
                DrawComponent(rect, prop, component, index);
            };
            
            
            list.drawElementHandleCallback = (rect, index, isActive, isFocused) =>
            {
                // Draw a small handle on the left side of the element
                rect.x += 5;
                rect.width = 20;
                rect.height = EditorGUIUtility.singleLineHeight;
                
                // Draw a hamburger icon
                GUI.Label(rect, "☰", new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter });
            };
        }
        
        public override void DrawCustomInspector()
        {
            base.DrawCustomInspector();
            
            EditorGUILayout.Space();
            
            serializedObject.Update();
            
            SnEditorGUI.DrawTitle("Movement Components");
            DrawComponents();
            DrawAddComponentButton();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawAddComponentButton()
        {
            // Draw a small plus "+" button that opens a popup with all available components
            var addedComponents = (target as FPSMovement).GetMovementComponents<MovementComponent>().Select(x => x.GetType()).ToArray();
            if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20)))
            {
                var menu = new UnityEditor.GenericMenu();
                var components = SnEditorUtils.GetSubclasses<MovementComponent>();
                foreach (var component in components)
                {
                    if (addedComponents.Contains(component)) continue;
                    menu.AddItem(new GUIContent(component.Name), false, () =>
                    {
                        var movement = (FPSMovement)target;
                        var newComponent = (MovementComponent)System.Activator.CreateInstance(component);
                        movement.AddComponent(newComponent);
                    });
                }
                menu.ShowAsContext();
            }
        }

        private void DrawComponents()
        {
            var movement = (FPSMovement)target;
            var components = movement.GetMovementComponents<MovementComponent>();
            if (components.Length == 0)
            {
                UnityEditor.EditorGUILayout.HelpBox("No components found", UnityEditor.MessageType.Warning);
                return;
            }
            else
            {
                // Draw a reorderable list with all the components
                list.DoList();
            }
        }
        
        private void DrawComponent(Rect rect, SerializedProperty property, MovementComponent component, int index)  
        {
            if (component == null)
            {
                // Remove the element if the component is null
                (target as FPSMovement).RemoveComponentAt(index);
                return;
            }
            
            // Draw a foldout with the component name
            var foldoutRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
            EditorGUILayout.BeginHorizontal();
            property.isExpanded = EditorGUI.Foldout(new Rect(foldoutRect.x, foldoutRect.y, foldoutRect.width - 20, foldoutRect.height),
                property.isExpanded, component.GetType().Name, true);
            
            // Draw a small "X" button to remove the component INSIDE the foldout
            if (GUI.Button(new Rect(foldoutRect.x + foldoutRect.width - 20, foldoutRect.y, 20, foldoutRect.height), "\u2715"))
            {
                (target as FPSMovement).RemoveComponent(component);
                return;
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Draw the component fields
            if (property.isExpanded)
            {
                // Draw the property
                EditorGUI.indentLevel++;
                // Add some space
                rect.y += EditorGUIUtility.standardVerticalSpacing;
                
                var fields = GetFields(component);
                var position = rect;
                foreach (var field in fields)
                {
                    // space for the property
                    position.y += EditorGUIUtility.standardVerticalSpacing;
                    var fieldProp = property.FindPropertyRelative(field.Name);
                    position.y += EditorGUI.GetPropertyHeight(fieldProp);
                    // Draw the property with the custom attributes
                    var label = new GUIContent(field.Name);
                    EditorGUI.PropertyField(position, fieldProp, label, true);
                }
                EditorGUI.indentLevel--;
            }
            
            rect.y += EditorGUIUtility.singleLineHeight;
            // Apply the changes
            serializedObject.ApplyModifiedProperties();
            // Redo record
            Undo.RecordObject(target, "Changed Movement Component");
        }
        
        private FieldInfo[] GetFields(MovementComponent component)
        {
            // Gets all component fields with attr SerializeField
            var fields = component.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            // Check if the field has the SerializeField attribute
            return System.Array.FindAll(fields, f => f.GetCustomAttributes(typeof(SerializeField), true).Length > 0);
        }
    }
}