using System.Collections;
using UnityEngine;

namespace LittleSword.Effects
{
    // ============================================================
    // Knockback: 피격 시 공격자 반대 방향으로 잠깐 밀려나는 효과
    // ============================================================
    // 사용법:
    //   Player, Enemy 프리팹에 컴포넌트 붙이기 (Rigidbody2D 필요)
    //   공격자가 ApplyFrom(공격자위치, 힘) 호출하면 자동 처리
    //
    // 동작:
    //   1. (자신 위치 - 공격자 위치) 방향으로 정규화
    //   2. linearVelocity에 잠깐 force 부여
    //   3. duration 후 속도 0으로 복구
    // ============================================================
    [RequireComponent(typeof(Rigidbody2D))]
    public class Knockback : MonoBehaviour
    {
        [SerializeField] private float defaultForce = 6f;     // 기본 밀림 세기
        [SerializeField] private float duration = 0.12f;      // 밀림 지속 시간
        [SerializeField] private bool isInvincibleDuringKnockback = false; // 옵션: 밀리는 동안 무적

        private Rigidbody2D rb;
        private Coroutine knockbackCoroutine;

        // 외부에서 읽기 전용 - 다른 시스템(이동 잠금 등)에서 활용 가능
        public bool IsKnockedBack { get; private set; }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        // 기본 힘으로 밀기
        public void ApplyFrom(Vector2 sourcePos) => ApplyFrom(sourcePos, defaultForce);

        // 힘 직접 지정
        public void ApplyFrom(Vector2 sourcePos, float force)
        {
            if (knockbackCoroutine != null) StopCoroutine(knockbackCoroutine);

            // 공격자 → 자신 방향 (즉, 공격자에게서 멀어지는 방향)
            Vector2 dir = ((Vector2)transform.position - sourcePos).normalized;

            // 방향이 0이면(같은 위치) 위쪽으로 밀어줌 (예외 처리)
            if (dir == Vector2.zero) dir = Vector2.up;

            knockbackCoroutine = StartCoroutine(KnockbackCoroutine(dir * force));
        }

        private IEnumerator KnockbackCoroutine(Vector2 velocity)
        {
            IsKnockedBack = true;
            rb.linearVelocity = velocity;

            yield return new WaitForSeconds(duration);

            rb.linearVelocity = Vector2.zero;
            IsKnockedBack = false;
            knockbackCoroutine = null;
        }
    }
}
