using UnityEngine;
using Logger = LittleSword.Common.Logger;

namespace LittleSword.Enemy.FSM
{
    // ============================================================
    // ChaseState: 적이 플레이어를 추격하는 상태
    // ============================================================
    // Idle 상태에서 플레이어를 감지하면 이 상태로 전환돼.
    //
    // 동작:
    //   - 달리기 애니메이션 시작
    //   - detectInterval마다 플레이어 위치 재탐지 및 이동
    //   - 공격 범위 안에 들어오면 → AttackState 전환
    //   - 플레이어를 놓치면 → Idlestate 전환
    // ============================================================
    public class ChaseState : IState
    {
        // 플레이어 재탐지 주기 (초)
        private readonly float detectInterval;

        // 마지막 탐지 시간 기록용
        private float lastDetectTime;

        // 생성자: 탐지 주기 설정 (기본값 0.3초)
        public ChaseState(float detectInterval = 0.3f)
        {
            this.detectInterval = detectInterval;
            // -detectInterval을 빼서 진입 직후 바로 첫 처리가 실행되게 함
            lastDetectTime = Time.time - detectInterval;
        }

        // Chase 상태로 진입할 때 한 번 실행
        public void Enter(Enemy enemy)
        {
            Logger.Log("Chase 진입");
            // 달리기 애니메이션 활성화 (true = 뛰는 모션 재생)
            enemy.animator.SetBool(Enemy.hashIsRun, true);
        }

        // Chase 상태 유지 중 매 프레임 실행
        public void Update(Enemy enemy)
        {
            // 아직 탐지 주기가 안 됐으면 스킵 (성능 최적화)
            if (Time.time - lastDetectTime < detectInterval) return;

            lastDetectTime = Time.time; // 탐지 시간 갱신

            Logger.Log("Chase 업데이트");

            if (enemy.DetectPlayer()) // 플레이어가 아직 범위 안에 있으면
            {
                enemy.MoveToPlayer(); // 플레이어 방향으로 이동

                // 공격 가능 거리 안에 들어왔으면 공격 상태로 전환
                if (enemy.IsInAttackRange())
                {
                    enemy.StopMoving();            // 이동 멈추고
                    enemy.ChangeState<AttackState>(); // 공격 시작!
                }
            }
            else // 플레이어가 감지 범위를 벗어났으면
            {
                enemy.StopMoving();          // 이동 멈추고
                enemy.ChangeState<Idlestate>(); // 다시 대기 상태로
            }
        }

        // Chase 상태에서 나갈 때 한 번 실행
        public void Exit(Enemy enemy)
        {
            Logger.Log("Chase 종료");
        }
    }
}
