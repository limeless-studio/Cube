using System;
using System.Collections;
using UnityEngine;

namespace Snowy.ProceduralAnimation
{
    [Serializable]
    public class ProceduralLeg
    {
        [SerializeField] private Transform legTarget;
        [SerializeField] private Transform tip;
        [SerializeField] private Vector3 targetOffset;
        
        private Vector3 defaultTargetPosition;
        Transform body;
        Transform transform;
        public bool isMoving;

        private Vector3 targetPosition;
        public Vector3 TargetPosition => targetPosition;

        public void Setup(Transform trans, Transform bdy)
        {
            transform = trans;
            targetPosition = legTarget.localPosition;
            defaultTargetPosition = targetPosition;
            legTarget.position = tip.position;
            body = bdy;
        }
        
        // Ground
        public void GroundLeg(Vector3 direction)
        {
            if (!legTarget || !tip || isMoving) return;
            RaycastHit hit;
            if (Physics.Raycast(legTarget.position + -direction, direction, out hit))
            {
                legTarget.position = hit.point;
            }
            else
            {
                // Check if anythinng is between the tip and the leg target
                if (Physics.Raycast(body.position, direction, out hit))
                {
                    Vector3 oldPos = legTarget.position;
                    legTarget.position = new Vector3(oldPos.x, hit.point.y, oldPos.z);
                }
            }
        }
        
        public void GroundTarget(Vector3 direction)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.TransformPoint(defaultTargetPosition) + transform.up, direction, out hit))
            {
                targetPosition = transform.InverseTransformPoint(hit.point);
            }
        }
        
        public float GetDistanceToTarget()
        {
            var pos = transform.TransformPoint(TargetPosition);
            return Vector3.Distance(legTarget.position, pos);
        }
        
        public IEnumerator MoveLeg(float smoothness, float stepHeight)
        {
            if (isMoving) yield break;
            isMoving = true;
            var offset = transform.TransformDirection(targetOffset);
            var pos = transform.TransformPoint(TargetPosition) + offset;
            var startPos = legTarget.position;
            var endPos = pos;
            
            // Check if endPos is grounded if not ground it
            RaycastHit hit;
            if (Physics.Raycast(endPos + transform.up, -transform.up, out hit))
            {
                endPos = hit.point;
                targetPosition = transform.InverseTransformPoint(endPos);
            }
            
            var t = 0f;
            while (t < 1)
            {
                t += Time.deltaTime / smoothness;
                var y = Mathf.Sin(t * Mathf.PI) * stepHeight;
                legTarget.position = Vector3.Lerp(startPos, endPos, t) + transform.up * y;
                yield return null;
            }
            isMoving = false;
        }
        
        # if UNITY_EDITOR
        public void DrawGizmos(Transform tans, float stepSize)
        {
            if (!transform) transform = tans;
            if (!legTarget) return;
            if (!Application.isPlaying) targetPosition = legTarget.localPosition;
            // Draw sphere around the TargetPosition
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.TransformPoint(TargetPosition), stepSize);
            
            // Draw target
            var offset = transform.TransformDirection(targetOffset);
            var pos = transform.TransformPoint(TargetPosition);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(legTarget.position, 0.1f);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(pos, 0.1f);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(pos + offset, 0.1f);
            
            // Draw grounding ray
            var shootingPos = transform.TransformPoint(TargetPosition) + transform.up;
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(shootingPos, -transform.up);
        }
        # endif
    }
    
    [Serializable] public class LegPair {
        public ProceduralLeg left;
        public ProceduralLeg right;
        public bool IsMoving => left is { isMoving: true } || right is { isMoving: true };
        
        public void Setup(Transform transform, Transform body) {
            left.Setup(transform, body);
            right.Setup(transform, body);
        }
        
        public void GroundLegs(Vector3 direction) {
            if (left == null || right == null) return;
            left.GroundLeg(direction);
            right.GroundLeg(direction);
        }
        
        public void GroundTargets(Vector3 direction)
        {
            if (left == null || right == null) return;
            left.GroundTarget(direction);
            right.GroundTarget(direction);
        }
        
        public float GetDistanceToTargets() {
            if (left == null || right == null) return 0;
            return Mathf.Max(left.GetDistanceToTarget(), right.GetDistanceToTarget());
        }
        
        public IEnumerator MoveLegs(float smoothness, float stepHeight) {
            if (IsMoving) yield break;
            var leftDistance = left.GetDistanceToTarget();
            var rightDistance = right.GetDistanceToTarget();
            if (leftDistance > rightDistance) {
                yield return left.MoveLeg(smoothness, stepHeight);
            } else {
                yield return right.MoveLeg(smoothness, stepHeight);
            }
        }
        
        public IEnumerator MoveLegs(float smoothness, float stepHeight, AudioClip stepSound, AudioSource source) {
            if (IsMoving) yield break;
            yield return MoveLegs(smoothness, stepHeight);
            source.clip = stepSound;
            source.Play();
        }
        
        # if UNITY_EDITOR
        public void DrawGizmos(Transform transform, float stepSize) {
            if (left != null) left.DrawGizmos(transform, stepSize);
            if (right != null) right.DrawGizmos(transform, stepSize);
        }
        
        # endif
    }
    
    public class ProceduralPairLegs : MonoBehaviour
    {
        [SerializeField] private LegPair[] legPairs;
        [SerializeField] private Transform body;
        [SerializeField] private AudioClip[] stepSounds;
        [SerializeField] private AudioSource audioSource;

        [SerializeField] private float stepSize = 0.5f;
        [SerializeField] private float smoothness = 1f;
        [SerializeField] private float stepHeight = 0.5f;

        private void Start()
        {
            foreach (var pair in legPairs)
            {
                pair.Setup(transform, body);
            }
            GroundLegs();
        }

        private void FixedUpdate()
        {
            GroundLegs();
            CheckLegToMove();
        }
        
        private void GroundLegs()
        {
            foreach (var pair in legPairs)
            {
                pair.GroundLegs(-transform.up);
                pair.GroundTargets(-transform.up);
            }
        }
        
        private void CheckLegToMove()
        {
            float maxDistance = stepSize;
            LegPair pairToMove = null;
            foreach (var pair in legPairs)
            {
                var distance = pair.GetDistanceToTargets();
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    pairToMove = pair;
                }
            }
            
            if (pairToMove != null && !pairToMove.IsMoving)
            {
                if (stepSounds.Length > 0 && audioSource)
                {
                    StartCoroutine(pairToMove.MoveLegs(smoothness, stepHeight, stepSounds[UnityEngine.Random.Range(0, stepSounds.Length)], audioSource));
                }
                else
                {
                    StartCoroutine(pairToMove.MoveLegs(smoothness, stepHeight));
                }
            }
        }

# if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (legPairs == null) return;
            foreach (var pair in legPairs)
            {
                pair.DrawGizmos(transform, stepSize);
            }
        }
        
        #endif
    }
}