using UnityEngine;

namespace LittleSword.Effects
{
    // ============================================================
    // HitFeedback: 타격 시 효과 묶음 (카메라 흔들림 + HitStop + Knockback)
    // ============================================================
    // 사용법 (공격하는 쪽에서 한 줄 호출):
    //   HitFeedback.Apply(victimCollider, transform.position);
    //
    // 흐름:
    //   1. 카메라 흔들림 (전체)
    //   2. 시간 정지 (전체)
    //   3. 피격자에게 Knockback 적용 (있을 때만)
    // ============================================================
    public static class HitFeedback
    {
        // 기본 호출
        public static void Apply(Collider2D victim, Vector2 attackerPos)
        {
            CameraShake.Instance?.Shake();
            HitStop.Instance?.Stop();
            victim?.GetComponent<Knockback>()?.ApplyFrom(attackerPos);
        }

        // 세기 직접 지정 (보스 충격파 등 강한 공격용)
        public static void Apply(Collider2D victim, Vector2 attackerPos,
                                 float shakeMagnitude, float stopDuration, float knockbackForce)
        {
            CameraShake.Instance?.Shake(0.15f, shakeMagnitude);
            HitStop.Instance?.Stop(stopDuration);
            victim?.GetComponent<Knockback>()?.ApplyFrom(attackerPos, knockbackForce);
        }
    }
}
