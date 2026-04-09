using UnityEngine;


namespace LittleSword.Controller
{
    // ============================================================
    // MovementController: 플레이어 이동을 처리하는 클래스
    // ============================================================
    // 이동 관련 로직만 담당하는 클래스야.
    // BasePlayer에서 이동 코드를 분리해서, 나중에 이동 방식을 바꿀 때
    // 이 파일만 수정하면 돼. (유지보수가 쉬워짐!)
    //
    // 참고: 이 클래스도 MonoBehaviour가 아니야!
    //   → new MovementController(rigidBody, spriteRenderer)로 생성해서 씀
    // ============================================================
    public class MovementController : MonoBehaviour
    {
        // 물리 이동에 사용할 Rigidbody2D (readonly = 생성 후 변경 불가)
        private readonly Rigidbody2D rigidBody;

        // 이동 방향에 따라 스프라이트를 좌우 반전하기 위한 SpriteRenderer
        private readonly SpriteRenderer spriteRenderer;

        // 생성자: Rigidbody2D와 SpriteRenderer를 외부에서 받아옴
        public MovementController(Rigidbody2D rigidBody, SpriteRenderer spriteRenderer)
        {
            this.rigidBody = rigidBody;
            this.spriteRenderer = spriteRenderer;
        }

        // 실제 이동을 처리하는 함수
        // direction: 이동 방향 벡터 (예: 오른쪽이면 (1,0), 왼쪽이면 (-1,0))
        // moveSpeed: 이동 속도 (PlayerStats에서 받아옴)
        public void Move(Vector2 direction, float moveSpeed)
        {
            // linearVelocity: Rigidbody의 속도를 직접 설정
            // 방향 × 속도 = 최종 이동 속도 벡터
            rigidBody.linearVelocity = direction * moveSpeed;

            // 이동 중일 때만 방향에 따라 스프라이트 반전
            if (direction != Vector2.zero)
            {
                // direction.x < 0: 왼쪽으로 이동 중이면 true → 스프라이트 좌우 반전
                // direction.x >= 0: 오른쪽이면 false → 반전 없음 (원본 방향)
                spriteRenderer.flipX = direction.x < 0;
            }
        }
    }
}
