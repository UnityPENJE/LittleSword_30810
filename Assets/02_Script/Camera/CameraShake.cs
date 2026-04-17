using System.Collections;
using UnityEngine;

namespace LittleSword.Effects
{
    // ============================================================
    // CameraShake: 카메라 흔들림 효과 (싱글톤)
    // ============================================================
    // 사용: CameraShake.Instance?.Shake();
    // Main Camera에 이 컴포넌트 붙이기
    // ============================================================
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        [SerializeField] private float defaultDuration = 0.15f;
        [SerializeField] private float defaultMagnitude = 0.1f;

        private Vector3 originalLocalPosition;
        private Coroutine shakeCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Shake() => Shake(defaultDuration, defaultMagnitude);

        public void Shake(float duration, float magnitude)
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                transform.localPosition = originalLocalPosition;
            }
            shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
        }

        private IEnumerator ShakeCoroutine(float duration, float magnitude)
        {
            originalLocalPosition = transform.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                Vector2 offset = Random.insideUnitCircle * magnitude;
                transform.localPosition = originalLocalPosition + new Vector3(offset.x, offset.y, 0f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = originalLocalPosition;
            shakeCoroutine = null;
        }
    }
}