using UnityEngine;

namespace LittleSword.Effects
{
    // ============================================================
    // CameraFollow: 타겟(플레이어)을 부드럽게 따라가는 카메라 스크립트
    // ============================================================
    // 사용법:
    //   1. Main Camera에 이 컴포넌트 추가
    //   2. target에 플레이어 Transform 드래그
    //
    // LateUpdate에서 움직이는 이유:
    //   플레이어 이동이 Update에서 처리되니까, 카메라는 그 다음인
    //   LateUpdate에서 따라가야 떨림 없이 자연스러워.
    // ============================================================
    public class CameraFollow : MonoBehaviour
    {
        [Header("추적 설정")]
        [SerializeField] private Transform target;          // 따라갈 대상 (플레이어)
        [SerializeField] private float smoothTime = 0.15f;  // 따라가는 부드러움 (작을수록 빠름)
        [SerializeField] private Vector2 offset = Vector2.zero; // 플레이어로부터 오프셋

        [Header("Z축 고정 (2D)")]
        [SerializeField] private float fixedZ = -10f;       // 카메라 Z값 (2D에서는 보통 -10)

        // SmoothDamp가 사용하는 내부 속도값 (ref 파라미터로 전달)
        private Vector3 velocity = Vector3.zero;

        private void LateUpdate()
        {
            if (target == null) return;

            // 목표 위치 = 타겟 위치 + 오프셋, Z는 고정
            Vector3 desired = new Vector3(
                target.position.x + offset.x,
                target.position.y + offset.y,
                fixedZ
            );

            // SmoothDamp: 현재 위치에서 목표 위치로 부드럽게 이동
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        }

        // 런타임 중 타겟 변경용 (멀티플레이 등)
        public void SetTarget(Transform newTarget) => target = newTarget;
    }
}