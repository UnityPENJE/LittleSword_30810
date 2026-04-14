using LittleSword.Interfaces;
using UnityEngine;

namespace LittleSword.Player
{
    // ============================================================
    // Lancer: 창병 직업 클래스
    // ============================================================
    // BasePlayer를 상속받아서 기본 기능은 공유하고,
    // 창병만의 전방 찌르기 공격을 추가로 구현했어.
    //
    // 공격 특성:
    //   - Warrior보다 좁지만 훨씬 긴 사각형 범위 (창 찌르기 느낌)
    //   - size = (2.5, 0.5): 가로가 길고 세로가 좁음
    //   - offset = 1.0f: 캐릭터에서 멀리 떨어진 곳까지 도달
    //
    // 공격 흐름:
    //   1. 플레이어가 공격 버튼 누름
    //   2. BasePlayer.Attack() → 공격 애니메이션 재생
    //   3. 애니메이션 이벤트 → OnLancerAttackEvent() 호출
    //   4. 전방 길고 좁은 OverlapBox 범위 안의 적에게 데미지!
    // ============================================================
    public class Lancer : BasePlayer
    {
        // 적 레이어 마스크 (인스펙터에서 Enemy 레이어 지정)
        [SerializeField] private LayerMask enemyLayer;

        // 공격 판정 사각형 크기: 가로(x)가 길고 세로(y)가 좁아서 창 찌르기 느낌
        [SerializeField] private Vector2 size = new Vector2(2.5f, 0.5f);

        // 공격 판정이 캐릭터 중심에서 얼마나 앞에 위치할지
        // Warrior(0.5f)보다 멀리 설정해서 더 긴 창 느낌
        [SerializeField] private float offset = 1.0f;

        // 애니메이션 이벤트에서 호출되는 창 찌르기 공격 함수
        // 유니티 Animation 클립에서 이 함수 이름을 Event로 등록해야 해
        public void OnLancerAttackEvent()
        {
            // 현재 바라보는 방향 계산 (스프라이트 좌우 반전으로 판단)
            Vector2 direction = spriteRenderer.flipX ? Vector2.left : Vector2.right;

            // 공격 판정 중심 위치 = 캐릭터 위치 + 전방 방향 * offset
            Vector2 center = (Vector2)transform.position + direction * offset;

            // OverlapBoxAll: 사각형 범위 안의 모든 콜라이더를 배열로 반환
            Collider2D[] colliders = Physics2D.OverlapBoxAll(center, size, 0, enemyLayer);

            // 범위 안의 모든 적에게 데미지 적용
            foreach (var col in colliders)
            {
                col.GetComponent<IDamageable>()?.TakeDamage(playerStats.attackDamage);
            }
        }

        // 에디터 Scene 뷰에서 공격 범위 시각화 (좁고 긴 박스로 표시됨)
        private void OnDrawGizmos()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

            Vector2 direction = spriteRenderer.flipX ? Vector2.left : Vector2.right;
            Vector2 center = (Vector2)transform.position + direction * offset;

            // 파란색: Warrior(빨강)와 구별해서 Lancer 공격 범위를 표시
            Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);
            Gizmos.DrawCube(center, new Vector3(size.x, size.y, 0f));
        }
    }
}
