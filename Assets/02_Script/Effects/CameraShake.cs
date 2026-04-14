using System.Collections;
using UnityEngine;

namespace LittleSword.Effects
{
    // ============================================================
    // CameraShake: 카메라 흔들림 효과를 담당하는 클래스
    // ============================================================
    // 플레이어가 피격당했을 때 화면이 흔들리면서 타격감을 줘.
    // 싱글톤 패턴으로 어디서든 CameraShake.Instance.Shake()로 호출 가능!
    //
    // 흔들림 원리:
    //   카메라의 localPosition을 랜덤 오프셋으로 움직였다가
    //   duration 시간 후 원래 위치로 복귀시켜.
    // ============================================================
    public class CameraShake : MonoBehaviour
    {
        // ─── 싱글톤 ──────────────────────────────────────────────
        // 씬에 하나만 존재하며, 어디서든 접근 가능한 전역 인스턴스
        public static CameraShake Instance { get; private set; }

        // ─── 기본 설정값 ─────────────────────────────────────────
        // 흔들림 지속 시간 (초)
        [SerializeField] private float defaultDuration = 0.15f;

        // 흔들림 세기 (값이 클수록 많이 흔들림)
        [SerializeField] private float defaultMagnitude = 0.1f;

        // 카메라의 원래 위치를 저장 (흔들림 끝나면 이 위치로 복귀)
        private Vector3 originalLocalPosition;

        // 현재 흔들림 코루틴 참조 (중복 실행 방지용)
        private Coroutine shakeCoroutine;

        private void Awake()
        {
            // 싱글톤 설정: 이미 인스턴스가 있으면 중복 오브젝트 제거
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        // 외부에서 호출하는 흔들림 함수 (기본값 사용 버전)
        public void Shake()
        {
            Shake(defaultDuration, defaultMagnitude);
        }

        // 흔들림 시간과 세기를 직접 지정할 수 있는 버전
        public void Shake(float duration, float magnitude)
        {
            // 이미 흔들리고 있으면 기존 코루틴 중지 후 위치 복구
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                transform.localPosition = originalLocalPosition;
            }

            shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
        }

        // 실제 흔들림을 처리하는 코루틴
        // 코루틴: yield return으로 여러 프레임에 걸쳐 실행할 수 있는 함수
        private IEnumerator ShakeCoroutine(float duration, float magnitude)
        {
            // 흔들림 시작 전 원래 위치 저장
            originalLocalPosition = transform.localPosition;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                // Random.insideUnitCircle: 반지름 1인 원 안의 랜덤 좌표 (Vector2)
                // magnitude를 곱해서 흔들림 세기 조절
                Vector2 offset = Random.insideUnitCircle * magnitude;

                // 원래 위치에 랜덤 오프셋을 더해서 카메라를 흔듦
                transform.localPosition = originalLocalPosition + new Vector3(offset.x, offset.y, 0f);

                elapsed += Time.deltaTime;

                // 다음 프레임까지 대기
                yield return null;
            }

            // 흔들림 끝나면 정확히 원래 위치로 복귀
            transform.localPosition = originalLocalPosition;
            shakeCoroutine = null;
        }
    }
}
