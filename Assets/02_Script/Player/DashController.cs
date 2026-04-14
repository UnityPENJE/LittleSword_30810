using System.Collections;
using UnityEngine;

namespace LittleSword.Player
{
    // ============================================================
    // DashController: 플레이어 대시(회피) 기능을 담당하는 클래스
    // ============================================================
    // 스페이스바를 누르면 현재 이동 방향으로 빠르게 대시해.
    // 대시 중에는 무적 상태(i-frame)가 되어 적의 공격을 받지 않아!
    //
    // 동작 흐름:
    //   1. 플레이어가 대시 버튼 누름
    //   2. 쿨다운 체크 → 사용 가능하면 대시 시작
    //   3. dashDuration 동안 빠른 속도로 이동 + 무적 상태
    //   4. 대시 종료 후 쿨다운 시작
    //
    // BasePlayer에서 TakeDamage 시 IsInvincible을 체크해서 무적 적용
    // ============================================================
    public class DashController : MonoBehaviour
    {
        // ─── 설정값 ──────────────────────────────────────────────
        // 대시 속도 (일반 이동보다 훨씬 빠르게)
        [SerializeField] private float dashSpeed = 15f;

        // 대시 지속 시간 (초) - 짧을수록 민첩한 느낌
        [SerializeField] private float dashDuration = 0.15f;

        // 대시 쿨다운 (초) - 연속 대시 방지
        [SerializeField] private float dashCooldown = 1.0f;

        // ─── 내부 상태 ───────────────────────────────────────────
        // Rigidbody2D 참조 (대시 시 velocity 직접 조작)
        private Rigidbody2D rigidBody;

        // 현재 대시 중인지 여부
        private bool isDashing = false;

        // 현재 무적 상태인지 여부 (BasePlayer.TakeDamage에서 참조)
        public bool IsInvincible { get; private set; }

        // 쿨다운 타이머 (0 이하면 대시 사용 가능)
        private float cooldownTimer = 0f;

        // 마지막 이동 방향 (대시 방향으로 사용)
        private Vector2 lastMoveDirection = Vector2.right;

        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            // 쿨다운 감소
            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
            }
        }

        // ─── 외부에서 호출하는 함수 ──────────────────────────────

        // 이동 방향을 업데이트 (BasePlayer.Move에서 호출)
        // 대시할 때 이 방향으로 날아감
        public void SetMoveDirection(Vector2 direction)
        {
            if (direction != Vector2.zero)
            {
                lastMoveDirection = direction.normalized;
            }
        }

        // 대시 실행 (InputHandler.OnDash 이벤트에 연결)
        public void TriggerDash()
        {
            // 이미 대시 중이거나 쿨다운 중이면 무시
            if (isDashing || cooldownTimer > 0f) return;

            StartCoroutine(PerformDash());
        }

        // 실제 대시를 처리하는 코루틴
        private IEnumerator PerformDash()
        {
            isDashing = true;
            IsInvincible = true;

            // 대시 방향으로 빠르게 이동
            rigidBody.linearVelocity = lastMoveDirection * dashSpeed;

            // dashDuration 동안 대기
            yield return new WaitForSeconds(dashDuration);

            // 대시 종료: 속도 초기화
            rigidBody.linearVelocity = Vector2.zero;

            isDashing = false;
            IsInvincible = false;

            // 쿨다운 시작
            cooldownTimer = dashCooldown;
        }
    }
}
