using System;
using System.Collections;
using UnityEngine;

namespace Snowy.ProceduralAnimation
{
    public class ProceduralLegs : MonoBehaviour
    {
        [SerializeField] private ProceduralLeg[] legs;
        [SerializeField] private Transform body;

        [SerializeField] private float stepSize = 0.5f;
        [SerializeField] private float smoothness = 1f;
        [SerializeField] private float stepHeight = 0.5f;

        private void Start()
        {
            foreach (var pair in legs)
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
            foreach (var pair in legs)
            {
                pair.GroundLeg(-transform.up);
                pair.GroundTarget(-transform.up);
            }
        }
        
        private void CheckLegToMove()
        {
            float maxDistance = stepSize;
            ProceduralLeg pairToMove = null;
            foreach (var pair in legs)
            {
                var distance = pair.GetDistanceToTarget();
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    pairToMove = pair;
                }
            }
            
            if (pairToMove != null && !pairToMove.isMoving)
            {
                StartCoroutine(pairToMove.MoveLeg(smoothness, stepHeight));
            }
        }

# if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (legs == null) return;
            foreach (var pair in legs)
            {
                pair.DrawGizmos(transform, stepSize);
            }
        }
        
        #endif
    }
}