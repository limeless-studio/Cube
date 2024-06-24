using Snowy.ActiveRagdoll.Data;
using Snowy.ActiveRagdoll.Ragdoll;
using Snowy.Utils;
using UnityEditor;
using UnityEngine;

namespace Snowy.ActiveRagdoll
{
    [CustomEditor(typeof(RagdollAnimator))]
    public class RagdollAnimatorEditor : Editor
    {
        RagdollAnimator m_ragdoll;
        private SerializedProperty m_animator;
        private SerializedProperty m_armature;
        private SerializedProperty m_jointsPowerProfile;
        
        // Motion Variables
        private SerializedProperty m_stepInfo;
        private SerializedProperty m_armsInfo;
        private SerializedProperty m_hipsForwardForce;
        
        // Joints Variables
        private SerializedProperty m_jointsPowerProfileType;
        private SerializedProperty m_setupOnStart;
        
        // Balance Variables
        private SerializedProperty m_balanceMode;
        
        // Upright Torque
        private SerializedProperty m_uprightTorque;
        private SerializedProperty m_uprightTorqueFunction;
        private SerializedProperty m_rotationTorque;
        
        // Manual Torque
        private SerializedProperty m_manualTorque;
        private SerializedProperty m_maxManualRotSpeed;
        private SerializedProperty m_torqueInput;
        
        // Freeze Rotations
        private SerializedProperty m_freezeRotationSpeed;
        
        void OnEnable()
        {
            m_ragdoll = (RagdollAnimator)target;
            m_animator = serializedObject.FindProperty("animator");
            m_armature = serializedObject.FindProperty("armature");
            m_jointsPowerProfile = serializedObject.FindProperty("jointsPowerProfile");
            m_jointsPowerProfileType = serializedObject.FindProperty("jointsPowerProfileType");
            m_setupOnStart = serializedObject.FindProperty("setupOnStart");
            
            // Motion Variables
            m_stepInfo = serializedObject.FindProperty("stepInfo");
            m_armsInfo = serializedObject.FindProperty("armsInfo");
            m_hipsForwardForce = serializedObject.FindProperty("hipsForwardForce");
            
            // Balance Variables
            m_balanceMode = serializedObject.FindProperty("balanceMode");
            
            // Upright Torque
            m_uprightTorque = serializedObject.FindProperty("uprightTorque");
            m_uprightTorqueFunction = serializedObject.FindProperty("uprightTorqueFunction");
            m_rotationTorque = serializedObject.FindProperty("rotationTorque");
            
            // Manual Torque
            m_manualTorque = serializedObject.FindProperty("manualTorque");
            m_maxManualRotSpeed = serializedObject.FindProperty("maxManualRotSpeed");
            m_torqueInput = serializedObject.FindProperty("torqueInput");
            
            // Freeze Rotations
            m_freezeRotationSpeed = serializedObject.FindProperty("freezeRotationSpeed");
        }
        
        public override void OnInspectorGUI()
        {
            // Draw default script field
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((RagdollAnimator)target), typeof(RagdollAnimator), false);
            
            SnEditorGUI.DrawTitle("Ragdoll Animator");
            
            if (m_ragdoll.animator == null)
                EditorGUILayout.HelpBox("Animator is not assigned.", MessageType.Warning);
            
            m_ragdollChecker();
            
            serializedObject.Update();

            EditorGUILayout.Space();
            DrawReferencesSection();
            EditorGUILayout.Space();
            DrawMotionSection();
            EditorGUILayout.Space();
            DrawBalanceSection();
            EditorGUILayout.Space();
            DrawJointsSection();
            EditorGUILayout.Space();
            
            serializedObject.ApplyModifiedProperties();
            
            if (GUILayout.Button("Setup Bones"))
                m_ragdoll.SetupBones();
            
            if (GUILayout.Button("Apply Joints Power Profile"))
                m_ragdoll.SetPowerProfileType((JointsPowerProfileType)m_jointsPowerProfileType.enumValueIndex);
            
        }

        private void DrawReferencesSection()
        {
            SnEditorGUI.BeginSection("References");
            
            EditorGUILayout.PropertyField(m_animator);
            EditorGUILayout.Space();
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(m_armature);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            SnEditorGUI.InlineEditor(m_jointsPowerProfile, typeof(Armature));
            
            SnEditorGUI.EndSection();
        }
        
        private void DrawMotionSection()
        {
            SnEditorGUI.BeginSection("Motion Settings");
            
            SnEditorGUI.InlineEditor(m_stepInfo, typeof(StepData));
            SnEditorGUI.InlineEditor(m_armsInfo, typeof(ArmsData));
            EditorGUILayout.PropertyField(m_hipsForwardForce);
            
            SnEditorGUI.EndSection();
        }

        private void DrawJointsSection()
        {
            SnEditorGUI.BeginSection("Joints Settings");
            
            EditorGUILayout.PropertyField(m_jointsPowerProfileType);
            EditorGUILayout.PropertyField(m_setupOnStart);
            
            SnEditorGUI.EndSection();
        }

        private void DrawBalanceSection()
        {
            SnEditorGUI.BeginSection("Balance Settings");
            // Draw Balance Mode
            var prev = m_balanceMode.enumValueIndex;
            int mode = (int)(BalanceMode)EditorGUILayout.EnumPopup("Balance Mode", (BalanceMode)m_balanceMode.enumValueIndex);
            
            if (mode != prev)
            {
                m_ragdoll.SetMode((BalanceMode)mode);
            }
            
            BalanceMode balanceMode = (BalanceMode)m_balanceMode.enumValueIndex;
            
            switch (balanceMode)
            {
                case BalanceMode.UprightTorque:
                    EditorGUILayout.PropertyField(m_uprightTorque);
                    EditorGUILayout.PropertyField(m_uprightTorqueFunction);
                    EditorGUILayout.PropertyField(m_rotationTorque);
                    break;

                case BalanceMode.ManualTorque:
                    EditorGUILayout.PropertyField(m_manualTorque);
                    EditorGUILayout.PropertyField(m_maxManualRotSpeed);
                    EditorGUILayout.PropertyField(m_torqueInput);
                    break;

                case BalanceMode.FreezeRotations:
                    EditorGUILayout.PropertyField(m_freezeRotationSpeed);
                    break;

                case BalanceMode.StabilizerJoint:
                    EditorGUILayout.HelpBox("Stabilizer Joint is not implemented yet.", MessageType.Warning);
                    break;

                case BalanceMode.None:
                    EditorGUILayout.HelpBox("None mode means the ragdoll will not try to balance itself.", MessageType.Info);
                    break;
            }
            SnEditorGUI.EndSection();
        }

        /// <summary>
        /// Checks if the m_ragdoll is correctly configured and not missing any fields.
        /// </summary>
        void m_ragdollChecker()
        {
            if (m_ragdoll.armature == null)
            {
                EditorGUILayout.HelpBox("m_ragdoll is not assigned.", MessageType.Warning);
                return;
            }
            
            // Pelvis
            if (m_ragdoll.armature.pelvis == null)
                EditorGUILayout.HelpBox("Pelvis is not assigned.", MessageType.Warning);
            else
            {
                string[] notAssigned = CheckBone(m_ragdoll.armature.pelvis);
                if (notAssigned.Length > 0)
                    EditorGUILayout.HelpBox("Pelvis is missing: " + string.Join(", ", notAssigned), MessageType.Warning);
            }
            
            // Spine
            if (m_ragdoll.armature.spine == null)
                EditorGUILayout.HelpBox("Spine is not assigned.", MessageType.Warning);
            else
            {
                string[] notAssigned = CheckBone(m_ragdoll.armature.spine);
                if (notAssigned.Length > 0)
                    EditorGUILayout.HelpBox("Spine is missing: " + string.Join(", ", notAssigned), MessageType.Warning);
            }
            
            // Head
            if (m_ragdoll.armature.head == null)
                EditorGUILayout.HelpBox("Head is not assigned.", MessageType.Warning);
            else
            {
                string[] notAssigned = CheckBone(m_ragdoll.armature.head);
                if (notAssigned.Length > 0)
                    EditorGUILayout.HelpBox("Head is missing: " + string.Join(", ", notAssigned), MessageType.Warning);
            }
            
            // Arms
            if (m_ragdoll.armature.arms == null || m_ragdoll.armature.arms.Length == 0)
                EditorGUILayout.HelpBox("Arms are not assigned.", MessageType.Warning);
            else
            {
                for (int i = 0; i < m_ragdoll.armature.arms.Length; i++)
                {
                    if (m_ragdoll.armature.arms[i].upper == null)
                        EditorGUILayout.HelpBox("Arm " + i + " upper is not assigned.", MessageType.Warning);
                    else
                    {
                        string[] notAssigned = CheckBone(m_ragdoll.armature.arms[i].upper);
                        if (notAssigned.Length > 0)
                            EditorGUILayout.HelpBox("Arm " + i + " upper is missing: " + string.Join(", ", notAssigned), MessageType.Warning);
                    }
                    
                    if (m_ragdoll.armature.arms[i].middle == null)
                        EditorGUILayout.HelpBox("Arm " + i + " middle is not assigned.", MessageType.Warning);
                    else
                    {
                        string[] notAssigned = CheckBone(m_ragdoll.armature.arms[i].middle);
                        if (notAssigned.Length > 0)
                            EditorGUILayout.HelpBox("Arm " + i + " middle is missing: " + string.Join(", ", notAssigned), MessageType.Warning);
                    }
                    
                    if (m_ragdoll.armature.arms[i].lower == null)
                        EditorGUILayout.HelpBox("Arm " + i + " lower is not assigned.", MessageType.Warning);
                    else
                    {
                        string[] notAssigned = CheckBone(m_ragdoll.armature.arms[i].lower);
                        if (notAssigned.Length > 0)
                            EditorGUILayout.HelpBox("Arm " + i + " lower is missing: " + string.Join(", ", notAssigned), MessageType.Warning);
                    }
                }
            }
            
            // Legs
            if (m_ragdoll.armature.legs == null || m_ragdoll.armature.legs.Length == 0)
                EditorGUILayout.HelpBox("Legs are not assigned.", MessageType.Warning);
            else
            {
                for (int i = 0; i < m_ragdoll.armature.legs.Length; i++)
                {
                    if (m_ragdoll.armature.legs[i].upper == null)
                        EditorGUILayout.HelpBox("Leg " + i + " upper is not assigned.", MessageType.Warning);
                    else
                    {
                        string[] notAssigned = CheckBone(m_ragdoll.armature.legs[i].upper);
                        if (notAssigned.Length > 0)
                            EditorGUILayout.HelpBox("Leg " + i + " upper is missing: " + string.Join(", ", notAssigned), MessageType.Warning);
                    }
                    
                    if (m_ragdoll.armature.legs[i].middle == null)
                        EditorGUILayout.HelpBox("Leg " + i + " middle is not assigned.", MessageType.Warning);
                    else
                    {
                        string[] notAssigned = CheckBone(m_ragdoll.armature.legs[i].middle);
                        if (notAssigned.Length > 0)
                            EditorGUILayout.HelpBox("Leg " + i + " middle is missing: " + string.Join(", ", notAssigned), MessageType.Warning);
                    }
                    
                    if (m_ragdoll.armature.legs[i].lower == null)
                        EditorGUILayout.HelpBox("Leg " + i + " lower is not assigned.", MessageType.Warning);
                    else
                    {
                        string[] notAssigned = CheckBone(m_ragdoll.armature.legs[i].lower);
                        if (notAssigned.Length > 0)
                            EditorGUILayout.HelpBox("Leg " + i + " lower is missing: " + string.Join(", ", notAssigned), MessageType.Warning);
                    }
                }
            }
        }

        string[] CheckBone(Bone bone)
        {
            string[] notAssigned = new string[0];
            
            if (bone.joint == null)
                ArrayUtility.Add(ref notAssigned, "Joint");
            
            if (bone.rigidbody == null)
                ArrayUtility.Add(ref notAssigned, "Rigidbody");
            
            if (bone.collider == null)
                ArrayUtility.Add(ref notAssigned, "Collider");
            
            if (bone.transform == null)
                ArrayUtility.Add(ref notAssigned, "Transform");
            
            return notAssigned;
        }
    }
}