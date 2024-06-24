using UnityEngine;

namespace Snowy.Actors
{
    public enum DamageCause
    {
        DamagedByActor,
        DamagedByEnvironment,
        DamagedByOther
    }
    
    public class Actor : MonoBehaviour, IDamageable
    {
        public float maxHealth = 100f;
        public bool startWithMaxHealth = true;
        public float startingHealth = 100f;
        
        public float Health { get; protected set; }
        public bool IsAlive => Health > 0;
        
        protected virtual void Awake()
        {
            Health = startWithMaxHealth ? maxHealth : startingHealth;
        }
        
        public virtual void TakeDamage(float damage)
        {
            TakeDamage(damage, DamageCause.DamagedByOther);
        }

        public virtual void TakeDamage(float damage, DamageCause cause)
        {
            Health = Mathf.Clamp(Health - damage, 0, maxHealth);
            if (Health <= 0) Die();
        }

        public virtual void Heal(float health)
        {
            Health = Mathf.Clamp(Health + health, 0, maxHealth);
        }

        public virtual void Die()
        {
            // TODO: Generic Death
        }

        public virtual void SetHealth(float health)
        {
            Health = Mathf.Clamp(health, 0, maxHealth);
        }

        public virtual float GetHealth() => Health;
    }
}