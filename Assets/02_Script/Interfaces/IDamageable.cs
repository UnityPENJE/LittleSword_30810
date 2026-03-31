
namespace LittleSword.Interfaces
{
    public interface IDamageable
    {
        bool IsDead { get; }
        int CurrentHP {  get; }
        void TakeDamage(int damage)
        {
            
        }
        void Die()
        {
            
        }
    }
}

