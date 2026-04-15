using System.Collections;
using UnityEngine;

namespace LittleSword.Effects
{
    // ============================================================
    // HitStop: 타격 순간 시간을 잠깐 멈춰 묵직한 타격감을 주는 효과
    // ============================================================
    // 원리:
    //   Time.timeScale을 0으로 만들면 게임 전체가 멈춰.
    //   몇 프레임 멈췄다가 다시 1로 복귀시켜서 "쿵!" 느낌을 줘.
    //
    // 주의:
    //   WaitForSecondsRealtime을 써야 함 (timeScale=0이라 일반 WaitForSeconds는 안 흐름)
    // ============================================================
    public class HitStop : MonoBehaviour
    {
        public static HitStop Instance { get; private set; }

        // 기본 정지 시간 (초). 0.05 ~ 0.1 사이가 자연스러움
        [SerializeField] private float defaultDuration = 0.06f;

        private Coroutine stopCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // 외부 호출용 - 기본값
        public void Stop() => Stop(defaultDuration);

        // 외부 호출용 - 시간 직접 지정
        public void Stop(float duration)
        {
            if (stopCoroutine != null) StopCoroutine(stopCoroutine);
            stopCoroutine = StartCoroutine(StopCoroutine(duration));
        }

        private IEnumerator StopCoroutine(float duration)
        {
            Time.timeScale = 0f;
            // Realtime: timeScale 영향 안 받는 실제 시간 대기
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1f;
            stopCoroutine = null;
        }
    }
}
