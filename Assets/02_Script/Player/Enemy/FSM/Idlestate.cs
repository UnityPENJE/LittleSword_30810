using UnityEngine;
using Logger = LittleSword.Common.Logger;

namespace LittleSword.Enemy.FSM
{
    // ============================================================
    // Idlestate: 적이 가만히 서서 주변을 탐색하는 상태
    // ============================================================
    // 적이 처음 생성되거나 플레이어를 놓쳤을 때 이 상태가 돼.
    //
    // 동작:
    //   - 이동 멈춤 (달리기 애니메이션 끄기)
    //   - detectInterval마다 주변에 플레이어가 있는지 확인
    //   - 플레이어 발견 → ChaseState로 전환
    //
    // 왜 매 프레임 탐지 안 하고 interval을 둘까?
    //   탐지(OverlapCircle)는 물리 연산이라 비용이 있어.
    //   매 프레임 하면 적이 많을수록 성능이 떨어져.
    //   0.3~0.5초마다 한 번만 체크하면 자연스럽고 성능도 좋아!
    // ============================================================
    public class Idlestate : IState
    {
        // 플레이어 탐지 주기 (초)
        private readonly float detectInterval;

        // 마지막으로 탐지한 시간 기록용
        private float lastDetectTime;

        // 생성자: 탐지 주기를 받아서 설정 (기본값 0.3초)
        public Idlestate(float detectInterval = 0.3f)
        {
            this.detectInterval = detectInterval;
            // -detectInterval을 빼서 Enter 직후 바로 첫 탐지가 실행되게 함
            lastDetectTime = Time.time - detectInterval;
        }

        // Idle 상태로 진입할 때 한 번 실행
        public void Enter(Enemy enemy)
        {
            Logger.Log("Idle 진입");
            // 달리기 애니메이션 끄기 (false = 대기 애니메이션으로 전환)
            enemy.animator.SetBool(Enemy.hashIsRun, false);
        }

        // Idle 상태 유지 중 매 프레임 실행
        public void Update(Enemy enemy)
        {
            // 마지막 탐지 이후 detectInterval이 지나지 않았으면 아직 탐지 안 함
            if (Time.time - lastDetectTime >= detectInterval)
            {
                Logger.Log("Idle 업데이트");
                lastDetectTime = Time.time; // 탐지 시간 갱신

                // 주변에 플레이어가 있으면 Chase 상태로 전환
                if (enemy.DetectPlayer())
                {
                    enemy.ChangeState<ChaseState>();
                }
            }
        }

        // Idle 상태에서 나갈 때 한 번 실행
        public void Exit(Enemy enemy)
        {
            Logger.Log("Idle 종료");
        }
    }
}
