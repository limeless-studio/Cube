using System.Collections.Generic;
using Snowy.UI;
using Snowy.Utils;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace Snowy.NotificationManager
{
    
    enum EventType
    {
        OnShow,
        OnHide
    }
    
    [CustomEditor(typeof(SnNotificationObject), true)]
    [CanEditMultipleObjects]
    public class SnNotificationObjectEditor : Editor
    {
        private SerializedProperty m_title;
        private SerializedProperty m_hasContent;
        private SerializedProperty m_isContainer;
        private SerializedProperty m_container;
        private SerializedProperty m_content;
        private SerializedProperty m_background;
        
        private EventType m_eventType = EventType.OnShow;
        protected SnNotificationObject m_notificationObject;
        private readonly Dictionary<Effect, bool> m_foldouts = new();
        
        protected virtual void OnEnable()
        {
            m_title = serializedObject.FindProperty("title");
            m_isContainer = serializedObject.FindProperty("isContainer");
            m_container = serializedObject.FindProperty("container");
            m_hasContent = serializedObject.FindProperty("hasContent");
            m_content = serializedObject.FindProperty("content");
            m_background = serializedObject.FindProperty("background");
            
            m_notificationObject = (SnNotificationObject) target;
        }
        
        public override void OnInspectorGUI()
        {
            if (m_notificationObject == null)
            {
                EditorGUILayout.HelpBox("Notification Object is null", MessageType.Error);
                return;
            }
            
            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Shows the notification for 5 seconds", MessageType.Info);
                
                // Begin disabled group
                bool isShowing = m_notificationObject.IsShowing;
                using (new EditorGUI.DisabledGroupScope(!isShowing))
                {
                    if (GUILayout.Button("Hide Notification"))
                    {
                        m_notificationObject.HideNotification();
                    }
                }
                // End disabled group
                
                EditorGUILayout.Space();

                using (new EditorGUI.DisabledGroupScope(isShowing))
                {
                    if (GUILayout.Button("Show Notification"))
                    {
                        m_notificationObject.ShowNotification<NotificationData>(new ()
                        {
                            title = "Title",
                            content = "Content",
                            duration = 5f,
                            data = null
                        });
                    }
                }

                EditorGUILayout.Space();
            }
            else
            {
                EditorGUILayout.HelpBox("Notification Preview is only available in Play Mode", MessageType.Info);
            }
            
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Notification Settings", SnGUI.skin.titleStyle);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            serializedObject.Update();

            DrawFields();
            
            serializedObject.ApplyModifiedProperties();
            
            EditorGUILayout.Space();
            
            DrawEffects();
            
        }

        protected virtual void DrawFields()
        {
            EditorGUILayout.PropertyField(m_title);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(m_isContainer);
            if (m_isContainer.boolValue)
            {
                EditorGUILayout.PropertyField(m_container);
            }
            
            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(m_hasContent);
            if (m_hasContent.boolValue)
            {
                EditorGUILayout.PropertyField(m_content);
            }
            
            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(m_background);
            
            EditorGUILayout.Space();
        }
        
        private void DrawEffects()
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Event Effects", SnGUI.skin.titleStyle);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            // Draw the tabs
            m_eventType = (EventType) GUILayout.Toolbar((int) m_eventType, new[] {"On Show", "On Hide"});
            switch (m_eventType)
            {
                case EventType.OnShow:
                    DrawEffectArea("On Show", m_notificationObject.onShowEffects);
                    break;
                case EventType.OnHide:
                    DrawEffectArea("On Hide", m_notificationObject.onHideEffects);
                    break;
            }
        }

        void DrawEffectArea(string label, EffectsGroup effectsGroup)
        {
            // Draw a dark box with a label and a foldout button
            EditorGUILayout.BeginVertical(SnGUI.skin.boxStyle);
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField(label);
            if (effectsGroup == null)
            {
                EditorGUILayout.HelpBox("EffectsGroup is null", MessageType.Error);
                return;
            }

            if (effectsGroup.isParallel)
            {
                EditorGUILayout.HelpBox("Parallel Effects", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Sequential Effects", MessageType.Info);
            }
            effectsGroup.isParallel = EditorGUILayout.Toggle("Parallel", effectsGroup.isParallel);
            
            EditorGUILayout.Space();
            
            var effects = effectsGroup.GetEffects();
            if (effects != null)
            {
                for (int i = 0; i < effects.Length; i++)
                {
                    // A foldout for each effect
                    var effect = effects[i];
                    m_foldouts.TryGetValue(effect, out var foldout);
                    
                    EditorGUILayout.BeginHorizontal();
                    m_foldouts[effect] = EditorGUILayout.Foldout(foldout, effect.GetType().Name);
                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        effectsGroup.RemoveEffect(effect);
                        m_foldouts.Remove(effect);
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    if (!foldout)
                    {
                        continue;
                    }
                    
                    EditorGUILayout.Space();
                    EditorGUI.indentLevel++;
                    // Draw the serialized properties of a normal effect
                    DrawEffectProperties(effect);
                    EditorGUI.indentLevel--;
                }
            }
            
            EditorGUILayout.Space();

            if (GUILayout.Button("Add Effect"))
            {
                // Show a context menu with all the effects
                var menu = new GenericMenu();
                foreach (var effectType in System.Enum.GetValues(typeof(EffectType)))
                {
                    // Cannout use ref or out parameters in lambdas
                    var type = (EffectType) effectType;
                    menu.AddItem(new GUIContent(type.ToString()), false, () =>
                    {
                        // undo for the effect
                        Undo.RecordObject(m_notificationObject, "Add Effect");
                        
                        var effect = effectsGroup.AddEffect(type);
                        m_foldouts.Add(effect, true);
                    });
                }
                
                menu.ShowAsContext();
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        private void DrawEffectProperties(Effect effect)
        {
            // Draw the properties of the effect
            // FadeEffect -> Duration
            // ScaleEffect -> Duration
            // RotateEffect -> Duration
            // MoveEffect -> Duration
            // ColorizeEffect -> Duration
            switch (effect)
            {
                case FadeEffect fadeEffect:
                    fadeEffect.duration = EditorGUILayout.FloatField("Duration", fadeEffect.duration);
                    fadeEffect.to = EditorGUILayout.FloatField("To", fadeEffect.to);
                    fadeEffect.forceFrom = EditorGUILayout.Toggle("Force From", fadeEffect.forceFrom);
                    if (fadeEffect.forceFrom)
                        fadeEffect.f_from = EditorGUILayout.FloatField("From", fadeEffect.f_from); 
                    
                    break;
                case ScaleEffect scaleEffect:
                    scaleEffect.duration = EditorGUILayout.FloatField("Duration", scaleEffect.duration);
                    scaleEffect.to = EditorGUILayout.Vector3Field("To", scaleEffect.to);
                    scaleEffect.forceFrom = EditorGUILayout.Toggle("Force From", scaleEffect.forceFrom);
                    if (scaleEffect.forceFrom)
                        scaleEffect.f_from = EditorGUILayout.Vector3Field("From", scaleEffect.f_from);
                    break;
                case RotateEffect rotateEffect:
                    rotateEffect.duration = EditorGUILayout.FloatField("Duration", rotateEffect.duration);
                    rotateEffect.to = EditorGUILayout.Vector3Field("To", rotateEffect.to);
                    rotateEffect.forceFrom = EditorGUILayout.Toggle("Force From", rotateEffect.forceFrom);
                    if (rotateEffect.forceFrom)
                        rotateEffect.f_from = EditorGUILayout.Vector3Field("From", rotateEffect.f_from);
                    break;
                case MoveEffect moveEffect:
                    moveEffect.duration = EditorGUILayout.FloatField("Duration", moveEffect.duration);
                    moveEffect.to = EditorGUILayout.Vector3Field("To", moveEffect.to);
                    moveEffect.forceFrom = EditorGUILayout.Toggle("Force From", moveEffect.forceFrom);
                    if (moveEffect.forceFrom)
                        moveEffect.f_from = EditorGUILayout.Vector3Field("From", moveEffect.f_from);
                    break;
                case ColorizeEffect colorizeEffect:
                    colorizeEffect.duration = EditorGUILayout.FloatField("Duration", colorizeEffect.duration);
                    colorizeEffect.to = EditorGUILayout.ColorField("To", colorizeEffect.to);
                    colorizeEffect.forceFrom = EditorGUILayout.Toggle("Force From", colorizeEffect.forceFrom);
                    if (colorizeEffect.forceFrom)
                        colorizeEffect.f_from = EditorGUILayout.ColorField("From", colorizeEffect.f_from);
                    break;
            }
        }
    }
}