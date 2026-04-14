using LittleSword.Interfaces;
using UnityEngine;

namespace LittleSword.Player
{
    // ============================================================
    // Healer: 힐러 직업 클래스
    // ============================================================
    // 공격: 약한 근접 공격 (Warrior 패턴, 작은 범위)
    // 회복: 주기적으로 주변 아군 HP 회복 (healCooldown마다 자동 발동)
    //
    // 공격 흐름:
    //   1. 공격 버튼 → BasePlayer.Attack() → 공격 애니메이션
    //   2. 애니메이션 이벤트 → OnHealerAttackEvent() → 근접 데미지
    //
    // 회복 흐름:
    //   - Update()에서 healCooldown마다 HealNearbyAllies() 자동 호출
    //   - OverlapCircleAll로 playerLayer 감지 → BasePlayer.Heal() 호출
    // ============================================================
    public class Healer : BasePlayer
    {
        [Header("공격 설정")]
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private Vector2 attackSize = new Vector2(0.8f, 1.5f); // Warrior보다 작은 범위
        [SerializeField] private float attackOffset = 0.4f;

        [Header("회복 설정")]
        [SerializeField] private LayerMask playerLayer;       // 아군 레이어
        [SerializeField] private float healRadius = 3.0f;     // 회복 범위 반경
        [SerializeField] private int healAmount = 15;         // 회복량
        [SerializeField] private float healCooldown = 5.0f;   // 회복 주기(초)

        private float nextHealTime = 0f;

        // ─── 자동 회복 (Update) ──────────────────────────────────────
        private void Update()
        {
            if (Time.time >= nextHealTime)
            {
                HealNearbyAllies();
                nextHealTime = Time.time + healCooldown;
            }
        }

        // 주변 아군 HP 회복
        private void HealNearbyAllies()
        {
            // OverlapCircleAll: 원형 범위 안의 모든 콜라이더 반환
            Collider2D[] allies = Physics2D.OverlapCircleAll(transform.position, healRadius, playerLayer);

            foreach (var ally in allies)
            {
                // 자기 자신은 회복에서 제외하고 싶으면 아래 주석 해제
                // if (ally.gameObject == gameObject) continue;

                ally.GetComponent<BasePlayer>()?.Heal(healAmount);
            }
        }

        // 애니메이션 이벤트 → 근접 공격 판정
        public void OnHealerAttackEvent()
        {
            Vector2 direction = spriteRenderer.flipX ? Vector2.left : Vector2.right;
            Vector2 center = (Vector2)transform.position + direction * attackOffset;

            Collider2D[] colliders = Physics2D.OverlapBoxAll(center, attackSize, 0, enemyLayer);
            foreach (var col in colliders)
            {
                col.GetComponent<IDamageable>()?.TakeDamage(playerStats.attackDamage);
            }
        }

        // Scene 뷰에서 공격 범위(노랑)와 회복 범위(초록) 시각화
        private void OnDrawGizmos()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

            // 공격 범위 (노란색)
            Vector2 direction = spriteRenderer.flipX ? Vector2.left : Vector2.right;
            Vector2 center = (Vector2)transform.position + direction * attackOffset;
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawCube(center, new Vector3(attackSize.x, attackSize.y, 0f));

            // 회복 범위 (초록색)
            Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
            Gizmos.DrawSphere(transform.position, healRadius);
        }
    }
}
