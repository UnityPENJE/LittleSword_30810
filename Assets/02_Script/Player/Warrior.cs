using LittleSword.Interfaces;
using UnityEngine;

namespace LittleSword.Player
{
    // ============================================================
    // Warrior: 전사 직업 클래스
    // ============================================================
    // BasePlayer를 상속받아서 기본 기능은 공유하고,
    // 전사만의 근접 공격(범위 판정)을 추가로 구현했어.
    //
    // 공격 흐름:
    //   1. 플레이어가 공격 버튼 누름
    //   2. BasePlayer.Attack()이 호출되어 공격 애니메이션 재생
    //   3. 애니메이션 특정 프레임에 OnWarriorAttack() 호출 (Animation Event)
    //   4. 캐릭터 앞쪽 사각형 범위 안의 적에게 데미지!
    //
    // OverlapBox = 특정 사각형 영역 안에 있는 콜라이더를 찾는 유니티 물리 함수
    // ============================================================
    public class Warrior : BasePlayer
    {
        // 공격이 적용될 레이어 (인스펙터에서 "Enemy" 레이어만 선택)
        // LayerMask를 쓰면 적에게만 데미지가 가고, 벽/바닥은 무시할 수 있어
        [SerializeField] private LayerMask enemyLayer;

        // 공격 판정 사각형의 크기 (너비, 높이)
        [SerializeField] private Vector2 size = new Vector2(1.0f, 2.0f);

        // 공격 판정이 캐릭터 중심에서 얼마나 앞에 위치할지 (오프셋)
        [SerializeField] private float offset = 0.5f;

        // 애니메이션 이벤트에서 호출되는 근접 공격 함수
        public void OnWarriorAttack()
        {
            // 스프라이트 flipX로 현재 바라보는 방향을 판단
            // flipX가 true면 왼쪽, false면 오른쪽을 보고 있음
            Vector2 direction = spriteRenderer.flipX ? Vector2.left : Vector2.right;

            // 공격 판정 사각형의 중심 위치 = 캐릭터 위치 + 앞방향 * offset 거리
            Vector2 center = (Vector2)transform.position + direction * offset;

            // Physics2D.OverlapBoxAll: 사각형 범위 안의 모든 콜라이더를 배열로 반환
            // 파라미터: (중심 위치, 크기, 회전각도, 감지할 레이어)
            Collider2D[] colliders = Physics2D.OverlapBoxAll(center, size, 0, enemyLayer);

            // 범위 안에 있는 모든 콜라이더(적)에게 데미지 적용
            foreach (var collider in colliders)
            {
                // GetComponent<IDamageable>(): 해당 오브젝트에서 IDamageable 인터페이스 컴포넌트를 가져옴
                // ?.TakeDamage(): 컴포넌트가 null이 아닐 때만 TakeDamage 호출 (null 체크 안전 연산)
                collider.GetComponent<IDamageable>()?.TakeDamage(playerStats.attackDamage);
            }
        }

        // 유니티 에디터에서 공격 범위를 시각적으로 보여주는 함수 (실제 게임에서는 안 보임)
        // Scene 뷰에서 반투명 빨간 박스로 공격 범위를 확인할 수 있어!
        private void OnDrawGizmos()
        {
            // spriteRenderer가 없으면 먼저 찾아옴 (에디터에서 Awake가 안 실행될 수도 있어)
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

            Vector2 direction = spriteRenderer.flipX ? Vector2.left : Vector2.right;
            Vector2 center = (Vector2)transform.position + direction * offset;

            // 반투명 빨간색으로 공격 범위 박스를 그림
            // new Color(R, G, B, A) - A는 투명도 (0 = 완전 투명, 1 = 불투명)
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawCube(center, new Vector3(size.x, size.y, 0.0f));
        }
    }
}
