using System.Collections;
using UnityEngine;

namespace LittleSword.Effects
{
    // ============================================================
    // HitFlash: 피격 시 스프라이트가 깜빡이는 효과를 담당하는 클래스
    // ============================================================
    // 적이나 플레이어가 피해를 받았을 때 스프라이트를 빠르게 깜빡여서
    // "맞았다!"는 시각적 피드백을 줘.
    //
    // 동작 원리:
    //   SpriteRenderer의 color를 흰색 ↔ 원래색으로 빠르게 전환했다가
    //   원래색으로 복귀시켜. (총 3번 깜빡임)
    // ============================================================
    public class HitFlash : MonoBehaviour
    {
        // ─── 설정값 ──────────────────────────────────────────────
        // 깜빡임 총 지속 시간 (초)
        [SerializeField] private float flashDuration = 0.15f;

        // 깜빡임 횟수 (짝수면 원래색으로 끝남)
        [SerializeField] private int flashCount = 4;

        // ─── 컴포넌트 ────────────────────────────────────────────
        private SpriteRenderer spriteRenderer;

        // 스프라이트의 원래 색상을 저장
        private Color originalColor;

        // 현재 깜빡임 코루틴 참조 (중복 실행 방지용)
        private Coroutine flashCoroutine;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            originalColor = spriteRenderer.color;
        }

        // 외부에서 호출하는 깜빡임 함수
        public void Flash()
        {
            // 이미 깜빡이고 있으면 기존 코루틴 중지 후 원래색 복구
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
                spriteRenderer.color = originalColor;
            }

            flashCoroutine = StartCoroutine(FlashCoroutine());
        }

        // 실제 깜빡임을 처리하는 코루틴
        private IEnumerator FlashCoroutine()
        {
            // 한 번 깜빡이는 데 걸리는 시간 = 총 시간 / 깜빡임 횟수
            float interval = flashDuration / flashCount;

            for (int i = 0; i < flashCount; i++)
            {
                // 짝수 번째: 흰색, 홀수 번째: 원래색
                // → 흰색 → 원래색 → 흰색 → 원래색 순서로 깜빡임
                spriteRenderer.color = (i % 2 == 0) ? Color.white : originalColor;

                yield return new WaitForSeconds(interval);
            }

            // 깜빡임 끝나면 원래 색상으로 확실히 복구
            spriteRenderer.color = originalColor;
            flashCoroutine = null;
        }
    }
}
