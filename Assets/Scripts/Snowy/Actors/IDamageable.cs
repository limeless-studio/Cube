namespace Snowy.Actors
{
    public interface IDamageable
    {
        void TakeDamage(float damage);

        void TakeDamage(float damage, DamageCause cause);
    }
}