using UnityEngine;


namespace LittleSword.Controller
{
    public class AnimationController : MonoBehaviour
    {
        private readonly Animator animator;

        private static readonly int hashIsRun = Animator.StringToHash("IsRun");
        private static readonly int hashAttack = Animator.StringToHash("Attack");
        private static readonly int hashDie = Animator.StringToHash("Die");
        private static readonly int hashHit = Animator.StringToHash("Hit");

        public AnimationController(Animator animator)
        {
            this.animator = animator;
        }

        public void Move(bool isMoving)
        {
            animator.SetBool(hashIsRun, isMoving);
        }

        public void Attack()
        {
            animator.SetTrigger(hashAttack);
        }

        public void Die()
        {
            animator.SetTrigger(hashDie);
        }

        public void Hit()
        {
            animator.SetTrigger(hashHit);
        }
    }
}

