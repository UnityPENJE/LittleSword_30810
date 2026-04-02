using LittleSword.Player;
using UnityEngine;
using Logger = LittleSword.Common.Logger;

namespace LittleSword.Enemy.FSM
{
    public class AttackState : IState
    {
        private readonly float attackCooldown;
        private float lastAttackTime;

        public AttackState(float attackCooldown = 1.0f)
        {
            this.attackCooldown = attackCooldown;
            lastAttackTime = Time.time - this.attackCooldown;
        }
        public void Enter(Enemy enemy)
        {
            Logger.Log("Attack 진입");
            enemy.animator.SetTrigger(Enemy.hashAttack);
        }
        public void Update(Enemy enemy)
        {
            Logger.Log("Attack 갱신");

            if(Time.time - lastAttackTime >= attackCooldown)
            {
                lastAttackTime = Time.time;

                if(enemy.Target == null || enemy.Target.GetComponent<BasePlayer>()?.IsDead == true)
                {
                    enemy.ChangeState<Idlestate>();
                    return;
                }

                if (enemy.IsInAttackRange())
                {
                    enemy.animator.SetBool(Enemy.hashIsRun, false);
                    enemy.SetFacing();
                    enemy.animator.SetTrigger(Enemy.hashAttack);
                }
                else
                {
                    enemy.ChangeState<ChaseState>();
                }
            }
        }
        public void Exit(Enemy enemy)
        {
            Logger.Log("Attack 종료");
        }
    }
}


