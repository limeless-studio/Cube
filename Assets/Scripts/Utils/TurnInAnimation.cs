using System.Collections;
using UnityEngine;

namespace Utils
{
     class TransformScale
    {
        public Transform Transform;
        public Vector3 StartScale;
    }
    
    public class TurnInAnimation : MonoBehaviour
    {
        [SerializeField] Transform[] objectsToTurnIn;
        [SerializeField] float turnInDuration = 1f;
        [SerializeField] bool inParallel = false;
        
        private TransformScale[] transformScales;
        
        private void Start()
        {
            transformScales = new TransformScale[objectsToTurnIn.Length];
            for (int i = 0; i < objectsToTurnIn.Length; i++)
            {
                transformScales[i] = new TransformScale
                {
                    Transform = objectsToTurnIn[i],
                    StartScale = objectsToTurnIn[i].localScale
                };
                
                objectsToTurnIn[i].localScale = Vector3.zero;
            }
            
            if (inParallel)
            {
                TurnInParallel();
            }
            else
            {
                StartCoroutine(SeriesTurnIn());
            }
        }
        
        void TurnInParallel()
        {
            foreach (var obj in transformScales)
            {
                StartCoroutine(TurnIn(obj));
            }
        }
        
        IEnumerator SeriesTurnIn()
        {
            foreach (var obj in transformScales)
            {
                yield return StartCoroutine(TurnIn(obj));
            }
        }
        
        IEnumerator TurnIn(TransformScale tr)
        {
            var elapsed = 0f;
            while (elapsed < turnInDuration)
            {
                tr.Transform.localScale = Vector3.Lerp(Vector3.zero, tr.StartScale, elapsed / turnInDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            tr.Transform.localScale = tr.StartScale;
        }
    }
}