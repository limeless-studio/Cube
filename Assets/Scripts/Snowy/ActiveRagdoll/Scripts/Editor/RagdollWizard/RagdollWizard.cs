using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Snowy.ActiveRagdoll.RagdollWizard
{
    public class ActiveRagdollCreatorWindow : EditorWindow
    {
        private Animator animator;
        private Transform pelvis;
        private Transform leftHips;
        private Transform leftKnee;
        private Transform leftFoot;
        private Transform rightHips;
        private Transform rightKnee;
        private Transform rightFoot;
        private Transform leftArm;
        private Transform leftElbow;
        private Transform rightArm;
        private Transform rightElbow;
        private Transform middleSpine;
        private Transform head;


        public float totalMass = 60;
        public float strength = 2000.0F;
        public float boneStrength = 200.0F;
        public float damping = 5.0F;

        Vector3 right = Vector3.right;
        Vector3 up = Vector3.up;
        Vector3 forward = Vector3.forward;

        Vector3 worldRight = Vector3.right;
        Vector3 worldUp = Vector3.up;
        Vector3 worldForward = Vector3.forward;
        public bool flipForward;
        string errorString = "";
        string helpString = "";
        private bool isValid;

        class BoneInfo
        {
            public string Name;

            public Transform Anchor;
            public BoneInfo Parent;

            public float MinLimit;
            public float MaxLimit;
            public float SwingLimit;

            public Vector3 Axis;
            public Vector3 NormalAxis;

            public float RadiusScale;
            public Type ColliderType;

            public ArrayList Children = new ArrayList();
            public float Density;
            public float SummedMass;// The mass of this and all children bodies
        }

        ArrayList bones;
        BoneInfo rootBone;

        private void Update()
        {
            errorString = CheckConsistency();
            CalculateAxes();

            if (errorString.Length != 0)
            {
                helpString =
                    "Assign the animator and make sure the avatar configurations is right\n";
            }
            else
            {
                helpString =
                    "Make sure your character is in T-Stand.\nMake sure the blue axis faces in the same direction the chracter is looking.\nUse flipForward to flip the direction";
            }

            isValid = errorString.Length == 0;
        }

        private void OnGUI()
        {
            if (helpString.Length > 0)
            {
                GUILayout.Box(helpString, GUILayout.ExpandWidth(true));
            }

            GUILayout.Label("Animator", EditorStyles.boldLabel);
            animator = (Animator)EditorGUILayout.ObjectField(animator, typeof(Animator), true);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.HelpBox(
                "Presets are optional. They can be used to set up the ragdoll with a set of predefined values.",
                MessageType.Info);
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            
            totalMass = EditorGUILayout.FloatField("Total Mass", totalMass);
            strength = EditorGUILayout.FloatField("Strength", strength);
            boneStrength = EditorGUILayout.FloatField("Bone Strength", boneStrength);
            damping = EditorGUILayout.FloatField("Damping", damping);
            flipForward = EditorGUILayout.Toggle("Flip Forward", flipForward);

            // Show disabled if we have an error
            GUI.enabled = isValid;
            if (GUILayout.Button("Create", GUILayout.ExpandWidth(true)))
            {
                Create();
            }

            GUI.enabled = true;

            if (!isValid)
            {
                GUILayout.Box(errorString, GUILayout.ExpandWidth(true));
            }
        }

        private void Create()
        {
            if (animator == null)
            {
                Debug.LogError("Missing animator");
                return;
            }
            Cleanup();
            BuildCapsules();
            AddBreastColliders();
            AddHeadCollider();

            BuildBodies();
            BuildJoints();
            CalculateMass();
            
            // Focus on the root
            if (rootBone.Anchor)
            {
                Selection.activeGameObject = animator.gameObject;
                SceneView.FrameLastActiveSceneView();
            }

            // Close the window
            SetupScripts();
            //Close();
        }

        private void SetupScripts()
        {
            // if has armautre, set up from animator
            //var armature = animator.GetComponent<RagdollArmature>() ?? Undo.AddComponent<RagdollArmature>(animator.gameObject);
            //if (armature)
            //{
            //    armature.SetUp_Animator(animator);
            //}
        }

        private void CreateBones()
        {
            // Assign the bones from the animator.
            pelvis = animator.GetBoneTransform(HumanBodyBones.Hips);

            leftHips = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            leftKnee = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            
            rightHips = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            rightKnee = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);

            leftArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            leftElbow = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);

            rightArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            rightElbow = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);

            middleSpine = animator.GetBoneTransform(HumanBodyBones.Spine);
            head = animator.GetBoneTransform(HumanBodyBones.Head);
        }

        string CheckConsistency()
        {
            if (animator == null)
            {
                return "Missing animator";
            }

            CreateBones();
            PrepareBones();
            Hashtable map = new Hashtable();
            foreach (BoneInfo bone in bones)
            {
                if (bone.Anchor)
                {
                    if (map[bone.Anchor] != null)
                    {
                        BoneInfo oldBone = (BoneInfo)map[bone.Anchor];
                        return String.Format("{0} and {1} may not be assigned to the same bone.", bone.Name, oldBone.Name);
                    }
                    map[bone.Anchor] = bone;
                }
            }

            foreach (BoneInfo bone in bones)
            {
                if (bone.Anchor == null)
                    return String.Format("{0} has not been assigned yet.\n", bone.Name);
            }

            return "";
        }

        [MenuItem("Snowy/Create Ragdoll", false, 2000)]
        [MenuItem("GameObject/Snowy/Create Ragdoll", false, -0)]
        static void CreateWindow()
        {
            GetWindow<ActiveRagdollCreatorWindow>(false, "Active Ragdoll Creator");
        }

        void DecomposeVector(out Vector3 tangentCompo, Vector3 outwardDir, Vector3 outwardNormal)
        {
            Vector3 normalCompo;
            outwardNormal = outwardNormal.normalized;
            normalCompo = outwardNormal * Vector3.Dot(outwardDir, outwardNormal);
            tangentCompo = outwardDir - normalCompo;
        }

        void CalculateAxes()
        {
            if (head != null && pelvis != null)
                up = CalculateDirectionAxis(pelvis.InverseTransformPoint(head.position));
            if (rightElbow != null && pelvis != null)
            {
                Vector3 removed;
                DecomposeVector(out removed, pelvis.InverseTransformPoint(rightElbow.position), up);
                right = CalculateDirectionAxis(removed);
            }

            forward = Vector3.Cross(right, up);
            if (flipForward)
                forward = -forward;
        }

        void PrepareBones()
        {
            if (pelvis)
            {
                worldRight = pelvis.TransformDirection(right);
                worldUp = pelvis.TransformDirection(up);
                worldForward = pelvis.TransformDirection(forward);
            }

            bones = new ArrayList();

            rootBone = new BoneInfo();
            rootBone.Name = "Pelvis";
            rootBone.Anchor = pelvis;
            rootBone.Parent = null;
            rootBone.Density = 2.5F;
            // create joint
            bones.Add(rootBone);

            AddMirroredJoint("Hips", leftHips, rightHips, "Pelvis", worldRight, worldForward, -20, 70, 30, typeof(CapsuleCollider), 0.3F, 1.5F);
            AddMirroredJoint("Knee", leftKnee, rightKnee, "Hips", worldRight, worldForward, -80, 0, 0, typeof(CapsuleCollider), 0.25F, 1.5F);
            AddMirroredJoint("Foot", leftFoot, rightFoot, "Knee", worldRight, worldForward, -45, 45, 25, typeof(CapsuleCollider), 0.2F, 1.5F);

            AddJoint("Middle Spine", middleSpine, "Pelvis", worldRight, worldForward, -20, 20, 10, null, 1, 2.5F);

            AddMirroredJoint("Arm", leftArm, rightArm, "Middle Spine", worldUp, worldForward, -80, 10, 90, typeof(CapsuleCollider), 0.25F, 1.0F);
            AddMirroredJoint("Elbow", leftElbow, rightElbow, "Arm", worldForward, worldUp, -90, 0, 0, typeof(CapsuleCollider), 0.20F, 1.0F);

            AddJoint("Head", head, "Middle Spine", worldRight, worldForward, -40, 25, 25, null, 1, 1.0F);
        }

        BoneInfo FindBone(string boneName)
        {
            foreach (BoneInfo bone in bones)
            {
                if (bone.Name == boneName)
                    return bone;
            }
            return null;
        }

        void AddMirroredJoint(string jointName, Transform leftAnchor, Transform rightAnchor, string parent, Vector3 worldTwistAxis, Vector3 worldSwingAxis, float minLimit, float maxLimit, float swingLimit, Type colliderType, float radiusScale, float density)
        {
            if (jointName == "Arm")
            {
                AddJoint("Left " + jointName, leftAnchor, parent, worldTwistAxis, worldSwingAxis,  minLimit, maxLimit, swingLimit, colliderType, radiusScale, density);
                AddJoint("Right " + jointName, rightAnchor, parent, worldTwistAxis, worldSwingAxis,-maxLimit, Mathf.Abs(minLimit), swingLimit, colliderType, radiusScale, density);
            }
            else
            {
                AddJoint("Left " + jointName, leftAnchor, parent, worldTwistAxis, worldSwingAxis, minLimit, maxLimit,
                    swingLimit, colliderType, radiusScale, density);
                AddJoint("Right " + jointName, rightAnchor, parent, worldTwistAxis, worldSwingAxis, minLimit, maxLimit,
                    swingLimit, colliderType, radiusScale, density);
            }
        }

        void AddJoint(string jointName, Transform anchor, string parent, Vector3 worldTwistAxis, Vector3 worldSwingAxis, float minLimit, float maxLimit, float swingLimit, Type colliderType, float radiusScale, float density)
        {
            BoneInfo bone = new BoneInfo();
            bone.Name = jointName;
            bone.Anchor = anchor;
            bone.Axis = worldTwistAxis;
            bone.NormalAxis = worldSwingAxis;
            bone.MinLimit = minLimit;
            bone.MaxLimit = maxLimit;
            bone.SwingLimit = swingLimit;
            bone.Density = density;
            bone.ColliderType = colliderType;
            bone.RadiusScale = radiusScale;

            if (FindBone(parent) != null)
                bone.Parent = FindBone(parent);
            else if (jointName.StartsWith("Left"))
                bone.Parent = FindBone("Left " + parent);
            else if (jointName.StartsWith("Right"))
                bone.Parent = FindBone("Right " + parent);


            bone.Parent.Children.Add(bone);
            bones.Add(bone);
        }

        void BuildCapsules()
        {
            foreach (BoneInfo bone in bones)
            {
                if (bone.ColliderType != typeof(CapsuleCollider))
                    continue;

                int direction;
                float distance;
                if (bone.Children.Count == 1)
                {
                    BoneInfo childBone = (BoneInfo)bone.Children[0];
                    Vector3 endPoint = childBone.Anchor.position;
                    CalculateDirection(bone.Anchor.InverseTransformPoint(endPoint), out direction, out distance);
                }
                else
                {
                    var position1 = bone.Anchor.position;
                    Vector3 endPoint = (position1 - bone.Parent.Anchor.position) + position1;
                    CalculateDirection(bone.Anchor.InverseTransformPoint(endPoint), out direction, out distance);

                    if (bone.Anchor.GetComponentsInChildren(typeof(Transform)).Length > 1)
                    {
                        Bounds bounds = new Bounds();
                        foreach (var component in bone.Anchor.GetComponentsInChildren(typeof(Transform)))
                        {
                            var child = (Transform)component;
                            bounds.Encapsulate(bone.Anchor.InverseTransformPoint(child.position));
                        }

                        if (distance > 0)
                            distance = bounds.max[direction];
                        else
                            distance = bounds.min[direction];
                    }
                }

                CapsuleCollider collider = Undo.AddComponent<CapsuleCollider>(bone.Anchor.gameObject);
                collider.direction = direction;

                Vector3 center = Vector3.zero;
                center[direction] = distance * 0.5F;
                collider.center = center;
                collider.height = Mathf.Abs(distance);
                collider.radius = Mathf.Abs(distance * bone.RadiusScale);
            }
        }

        void Cleanup()
        {
            foreach (BoneInfo bone in bones)
            {
                if (!bone.Anchor)
                    continue;

                Component[] joints = bone.Anchor.GetComponentsInChildren(typeof(Joint));
                foreach (var component in joints)
                {
                    var joint = (Joint)component;
                    Undo.DestroyObjectImmediate(joint);
                }

                Component[] bodies = bone.Anchor.GetComponentsInChildren(typeof(Rigidbody));
                foreach (var component in bodies)
                {
                    var body = (Rigidbody)component;
                    Undo.DestroyObjectImmediate(body);
                }

                Component[] colliders = bone.Anchor.GetComponentsInChildren(typeof(Collider));
                foreach (var component in colliders)
                {
                    var collider = (Collider)component;
                    Undo.DestroyObjectImmediate(collider);
                }
            }
        }

        void BuildBodies()
        {
            foreach (BoneInfo bone in bones)
            {
                Undo.AddComponent<Rigidbody>(bone.Anchor.gameObject);
                bone.Anchor.GetComponent<Rigidbody>().mass = bone.Density;
            }
        }

        void BuildJoints()
        {
            foreach (BoneInfo bone in bones)
            {
                if (bone.Name == "Pelvis")
                {
                    ConfigurableJoint rootJoint = pelvis.gameObject.AddComponent<ConfigurableJoint>();
                    rootJoint.connectedBody = null;
                    rootJoint.axis = worldForward;
                    rootJoint.secondaryAxis = worldUp;
                    rootJoint.xMotion = ConfigurableJointMotion.Free;
                    rootJoint.yMotion = ConfigurableJointMotion.Free;
                    rootJoint.zMotion = ConfigurableJointMotion.Free;
            
                    rootJoint.rotationDriveMode = RotationDriveMode.Slerp;
                    JointDrive drive2 = new JointDrive();
                    drive2.positionSpring = strength;
                    drive2.positionDamper = damping;
                    drive2.maximumForce = Mathf.Infinity;
            
                    rootJoint.slerpDrive = drive2;
                    
                    rootJoint.projectionMode = JointProjectionMode.PositionAndRotation;
                }
                
                
                
                if (bone.Parent == null)
                    continue;

                ConfigurableJoint joint = Undo.AddComponent<ConfigurableJoint>(bone.Anchor.gameObject);

                // Setup connection and axis
                joint.axis = CalculateDirectionAxis(bone.Anchor.InverseTransformDirection(bone.Axis));
                // joint.swingAxis = CalculateDirectionAxis(bone.anchor.InverseTransformDirection(bone.normalAxis));
                joint.secondaryAxis = CalculateDirectionAxis(bone.Anchor.InverseTransformDirection(Vector3.Cross(bone.Axis, bone.NormalAxis)));
                joint.anchor = Vector3.zero;
                joint.connectedBody = bone.Parent.Anchor.GetComponent<Rigidbody>();
                joint.enablePreprocessing = false; // turn off to handle degenerated scenarios, like spawning inside geometry.

                // Setup limits
                SoftJointLimit limit = new SoftJointLimit();
                limit.contactDistance = 0; // default to zero, which automatically sets contact distance.

                limit.limit = bone.MinLimit;
                // joint.lowTwistLimit = limit;
                joint.lowAngularXLimit = limit;

                limit.limit = bone.MaxLimit;
                //joint.highTwistLimit = limit;
                joint.highAngularXLimit = limit;

                limit.limit = bone.SwingLimit;
                // joint.swing1Limit = limit;
                joint.angularYLimit = limit;

                if (bone.Name.Contains("Arm") || bone.Name.Contains("Elbow"))
                {
                    limit.limit = 90;
                }
                else
                {
                    limit.limit = 0;
                }
                
                // joint.swing2Limit = limit;
                joint.angularZLimit = limit;
                
                // Setup drive
                JointDrive drive = new JointDrive();
                drive.positionSpring = boneStrength;
                drive.positionDamper = 0f;
                drive.maximumForce = Mathf.Infinity;
                
                joint.angularXDrive = drive;
                joint.angularYZDrive = drive;
                joint.rotationDriveMode = RotationDriveMode.Slerp;
                
                drive.positionSpring = strength * 0.1f;
                drive.positionDamper = damping;
                drive.maximumForce = Mathf.Infinity;
                joint.slerpDrive = drive;

                // lock motion in the joint
                joint.angularXMotion = ConfigurableJointMotion.Limited;
                joint.angularYMotion = ConfigurableJointMotion.Limited;
                joint.angularZMotion = ConfigurableJointMotion.Limited;
                joint.xMotion = ConfigurableJointMotion.Locked;
                joint.yMotion = ConfigurableJointMotion.Locked;
                joint.zMotion = ConfigurableJointMotion.Locked;
                
                joint.projectionMode = JointProjectionMode.PositionAndRotation;
            }
        }

        void CalculateMassRecurse(BoneInfo bone)
        {
            float mass = bone.Anchor.GetComponent<Rigidbody>().mass;
            foreach (BoneInfo child in bone.Children)
            {
                CalculateMassRecurse(child);
                mass += child.SummedMass;
            }
            bone.SummedMass = mass;
        }

        void CalculateMass()
        {
            // Calculate allChildMass by summing all bodies
            CalculateMassRecurse(rootBone);

            // Rescale the mass so that the whole character weights totalMass
            float massScale = totalMass / rootBone.SummedMass;
            foreach (BoneInfo bone in bones)
                bone.Anchor.GetComponent<Rigidbody>().mass *= massScale;

            // Recalculate allChildMass by summing all bodies
            CalculateMassRecurse(rootBone);
        }

        static void CalculateDirection(Vector3 point, out int direction, out float distance)
        {
            // Calculate longest axis
            direction = 0;
            if (Mathf.Abs(point[1]) > Mathf.Abs(point[0]))
                direction = 1;
            if (Mathf.Abs(point[2]) > Mathf.Abs(point[direction]))
                direction = 2;

            distance = point[direction];
        }

        static Vector3 CalculateDirectionAxis(Vector3 point)
        {
            int direction;
            float distance;
            CalculateDirection(point, out direction, out distance);
            Vector3 axis = Vector3.zero;
            if (distance > 0)
                axis[direction] = 1.0F;
            else
                axis[direction] = -1.0F;
            return axis;
        }

        static int SmallestComponent(Vector3 point)
        {
            int direction = 0;
            if (Mathf.Abs(point[1]) < Mathf.Abs(point[0]))
                direction = 1;
            if (Mathf.Abs(point[2]) < Mathf.Abs(point[direction]))
                direction = 2;
            return direction;
        }

        static int LargestComponent(Vector3 point)
        {
            int direction = 0;
            if (Mathf.Abs(point[1]) > Mathf.Abs(point[0]))
                direction = 1;
            if (Mathf.Abs(point[2]) > Mathf.Abs(point[direction]))
                direction = 2;
            return direction;
        }

        Bounds Clip(Bounds bounds, Transform relativeTo, Transform clipTransform, bool below)
        {
            int axis = LargestComponent(bounds.size);

            if (Vector3.Dot(worldUp, relativeTo.TransformPoint(bounds.max)) > Vector3.Dot(worldUp, relativeTo.TransformPoint(bounds.min)) == below)
            {
                Vector3 min = bounds.min;
                min[axis] = relativeTo.InverseTransformPoint(clipTransform.position)[axis];
                bounds.min = min;
            }
            else
            {
                Vector3 max = bounds.max;
                max[axis] = relativeTo.InverseTransformPoint(clipTransform.position)[axis];
                bounds.max = max;
            }
            return bounds;
        }

        Bounds GetBreastBounds(Transform relativeTo)
        {
            // Pelvis bounds
            Bounds bounds = new Bounds();
            bounds.Encapsulate(relativeTo.InverseTransformPoint(leftHips.position));
            bounds.Encapsulate(relativeTo.InverseTransformPoint(rightHips.position));
            bounds.Encapsulate(relativeTo.InverseTransformPoint(leftArm.position));
            bounds.Encapsulate(relativeTo.InverseTransformPoint(rightArm.position));
            Vector3 size = bounds.size;
            size[SmallestComponent(bounds.size)] = size[LargestComponent(bounds.size)] / 2.0F;
            bounds.size = size;
            return bounds;
        }

        void AddBreastColliders()
        {
            // Middle spine and pelvis
            if (middleSpine != null && pelvis != null)
            {
                Bounds bounds;
                BoxCollider box;

                // Middle spine bounds
                bounds = Clip(GetBreastBounds(pelvis), pelvis, middleSpine, false);
                box = Undo.AddComponent<BoxCollider>(pelvis.gameObject);
                box.center = bounds.center;
                box.size = bounds.size;

                bounds = Clip(GetBreastBounds(middleSpine), middleSpine, middleSpine, true);
                box = Undo.AddComponent<BoxCollider>(middleSpine.gameObject);
                box.center = bounds.center;
                box.size = bounds.size;
            }
            // Only pelvis
            else
            {
                Bounds bounds = new Bounds();
                bounds.Encapsulate(pelvis.InverseTransformPoint(leftHips.position));
                bounds.Encapsulate(pelvis.InverseTransformPoint(rightHips.position));
                bounds.Encapsulate(pelvis.InverseTransformPoint(leftArm.position));
                bounds.Encapsulate(pelvis.InverseTransformPoint(rightArm.position));

                Vector3 size = bounds.size;
                size[SmallestComponent(bounds.size)] = size[LargestComponent(bounds.size)] / 2.0F;

                BoxCollider box = Undo.AddComponent<BoxCollider>(pelvis.gameObject);
                box.center = bounds.center;
                box.size = size;
            }
        }

        void AddHeadCollider()
        {
            if (head.GetComponent<Collider>())
                Destroy(head.GetComponent<Collider>());

            float radius = Vector3.Distance(leftArm.transform.position, rightArm.transform.position);
            radius /= 4;

            SphereCollider sphere = Undo.AddComponent<SphereCollider>(head.gameObject);
            sphere.radius = radius;
            Vector3 center = Vector3.zero;

            int direction;
            float distance;
            CalculateDirection(head.InverseTransformPoint(pelvis.position), out direction, out distance);
            if (distance > 0)
                center[direction] = -radius;
            else
                center[direction] = radius;
            sphere.center = center;
        }
    }
}