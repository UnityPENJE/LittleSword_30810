using LittleSword.Player;
using UnityEngine;
using Logger = LittleSword.Common.Logger;

namespace LittleSword.Enemy.FSM
{
    // ============================================================
    // AttackState: 적이 플레이어를 공격하는 상태
    // ============================================================
    // 플레이어가 공격 범위 안에 들어오면 Chase에서 이 상태로 전환돼.
    //
    // 동작:
    //   - 공격 애니메이션 재생
    //   - attackCooldown마다 공격 조건 재확인
    //   - 플레이어가 범위 안이면 계속 공격
    //   - 플레이어가 범위를 벗어나면 → ChaseState 전환
    //   - 플레이어가 죽거나 없어지면 → Idlestate 전환
    //
    // 실제 데미지는 AnimationEvent로!
    //   공격 애니메이션의 특정 프레임에서 Enemy.OnAttackAnimationEvent()가 호출되고,
    //   거기서 target에게 TakeDamage()를 적용해.
    // ============================================================
    public class AttackState : IState
    {
        // 공격 쿨다운 (초) - 이 시간마다 한 번씩 공격
        private readonly float attackCooldown;

        // 마지막 공격 시간 기록용
        private float lastAttackTime;

        // 생성자: 공격 쿨다운 설정 (기본값 1초)
        public AttackState(float attackCooldown = 1.0f)
        {
            this.attackCooldown = attackCooldown;
            // -attackCooldown을 빼서 진입 즉시 첫 공격 가능하게 함
            lastAttackTime = Time.time - this.attackCooldown;
        }

        // Attack 상태로 진입할 때 한 번 실행
        public void Enter(Enemy enemy)
        {
            Logger.Log("Attack 진입");
            // 진입 즉시 공격 애니메이션 재생
            enemy.animator.SetTrigger(Enemy.hashAttack);
        }

        // Attack 상태 유지 중 매 프레임 실행
        public void Update(Enemy enemy)
        {
            Logger.Log("Attack 업데이트");

            // 쿨다운이 지났을 때만 다음 공격 처리
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                lastAttackTime = Time.time; // 공격 시간 갱신

                // 타겟이 없거나 죽었으면 Idle로 복귀
                if (enemy.Target == null || enemy.Target.GetComponent<BasePlayer>()?.IsDead == true)
                {
                    enemy.ChangeState<Idlestate>();
                    return;
                }

                if (enemy.IsInAttackRange()) // 아직 공격 범위 안에 있으면
                {
                    enemy.animator.SetBool(Enemy.hashIsRun, false); // 달리기 애니 끄기
                    enemy.SetFacing();                               // 플레이어 방향 바라보기
                    enemy.animator.SetTrigger(Enemy.hashAttack);    // 공격 애니메이션 재생
                }
                else // 플레이어가 도망갔으면
                {
                    enemy.ChangeState<ChaseState>(); // 다시 추격 상태로
                }
            }
        }

        // Attack 상태에서 나갈 때 한 번 실행
        public void Exit(Enemy enemy)
        {
            Logger.Log("Attack 종료");
        }
    }
}
