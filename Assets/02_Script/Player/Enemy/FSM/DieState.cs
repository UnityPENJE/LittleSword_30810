using UnityEngine;
using LittleSword.Player;

namespace LittleSword.Enemy.FSM
{
    // ============================================================
    // DieState: 적이 사망했을 때의 상태
    // ============================================================
    // HP가 0이 되면 Enemy.Die()에서 이 상태로 전환돼.
    //
    // Enter에서 모든 사망 처리를 한 번에 해:
    //   1. 사망 애니메이션 재생
    //   2. 이동 멈춤
    //   3. 충돌 비활성화 (다른 오브젝트가 뚫고 지나갈 수 있게)
    //   4. 물리 비활성화 (더 이상 물리 영향 안 받게)
    //   5. 5초 후 게임 오브젝트 자동 삭제
    //
    // Update와 Exit는 사망 후에 더 이상 처리할 게 없어서 빈 상태야.
    // ============================================================
    public class DieState : IState
    {
        // 사망 상태 진입 시 한 번만 실행되는 처리
        public void Enter(Enemy enemy)
        {
            // 사망 애니메이션 트리거 발동 (쓰러지는 모션 재생)
            enemy.animator.SetTrigger(Enemy.hashDie);

            // 이동 속도를 0으로 만들어 즉시 멈춤
            enemy.StopMoving();

            // Collider 비활성화: 죽은 적은 충돌 판정이 없어야 함
            // GetComponent: Enemy 게임 오브젝트에서 Collider2D 컴포넌트를 가져옴
            enemy.GetComponent<Collider2D>().enabled = false;

            // Kinematic 모드: 물리 엔진의 영향을 받지 않게 함
            // (중력, 충돌 반응 등이 사라짐)
            enemy.rigidbody.bodyType = RigidbodyType2D.Kinematic;

            // 플레이어에게 경험치 지급
            // FindObjectOfType: 씬에서 BasePlayer를 가진 오브젝트를 찾음
            BasePlayer player = Object.FindObjectOfType<BasePlayer>();
            if (player != null)
            {
                LevelSystem levelSystem = player.GetComponent<LevelSystem>();
                levelSystem?.AddXP(enemy.EnemyStats.expReward);
            }

            // 5초 후 이 게임 오브젝트를 씬에서 완전히 제거
            // Object.Destroy(오브젝트, 딜레이): 딜레이(초) 후에 삭제
            Object.Destroy(enemy.gameObject, 5.0f);
        }

        // 사망 후에는 Update 처리 없음 (오브젝트가 곧 삭제되므로)
        public void Update(Enemy enemy) { }

        // 사망 상태에서 빠져나가는 경우가 없으므로 Exit 처리 없음
        public void Exit(Enemy enemy) { }
    }
}
