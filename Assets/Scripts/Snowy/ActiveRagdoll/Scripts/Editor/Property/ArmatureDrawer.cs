using Snowy.ActiveRagdoll.Ragdoll;
using Snowy.Utils;
using UnityEditor;
using UnityEngine;

namespace Snowy.ActiveRagdoll.Property
{
    [CustomPropertyDrawer(typeof(Bone))]
    public class BoneDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var indent = EditorGUI.indentLevel;

            // Bone title & foldout
            var foldoutRect  = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);
            
            if (property.isExpanded)
                DrawFields(position, property);

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        void DrawFields(Rect position, SerializedProperty property)
        {
            // box
            var style = SnGUI.skin.boxStyle;
            var boxRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width,
                EditorGUIUtility.singleLineHeight * 4.2f + EditorGUIUtility.standardVerticalSpacing * 4);
            GUI.Box(boxRect, GUIContent.none, style);
            EditorGUI.indentLevel++;

            // Joint
            var jointRect = new Rect(position.x,
                position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
                position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(jointRect, property.FindPropertyRelative("joint"));

            // Rigidbody
            var rigidbodyRect = new Rect(position.x,
                jointRect.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
                position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(rigidbodyRect, property.FindPropertyRelative("rigidbody"));

            // Collider
            var colliderRect = new Rect(position.x,
                rigidbodyRect.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
                position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(colliderRect, property.FindPropertyRelative("collider"));

            // Transform
            var transformRect = new Rect(position.x,
                colliderRect.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
                position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(transformRect, property.FindPropertyRelative("transform"));

            EditorGUI.indentLevel--;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return property.isExpanded
                ? EditorGUIUtility.singleLineHeight * 5.2f + EditorGUIUtility.standardVerticalSpacing * 5
                : EditorGUIUtility.singleLineHeight;
        }
    }
    
}