using UnityEngine;

namespace Snowy.Actors
{
    public class BodyPart : MonoBehaviour, IDamageable
    {
        private Actor Actor;
        
        private void Awake()
        {
            Actor = GetComponentInParent<Actor>();
            if (Actor == null)
            {
                Debug.LogError("BodyPart must be a child of an Actor");
                Destroy(gameObject);
            }
        }
        
        public void TakeDamage(float damage)
        {
            Actor.TakeDamage(damage);
        }

        public void TakeDamage(float damage, DamageCause cause)
        {
            Actor.TakeDamage(damage, cause);
        }

        public void Heal(float health)
        {
            Actor.Heal(health);
        }

        public void Die()
        {
            Actor.Die();
        }

        public void SetHealth(float health)
        {
            Actor.SetHealth(health);
        }

        public float GetHealth()
        {
            return Actor.GetHealth();
        }
    }
}