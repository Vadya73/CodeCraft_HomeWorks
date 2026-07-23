namespace Game.Scripts.Combat.Common
{
    public interface IDamageable
    {
        TeamType Team { get; }
        bool IsAlive { get; }

        void TakeDamage(int damage);
    }
}
