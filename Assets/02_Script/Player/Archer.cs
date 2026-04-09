using LittleSword.Interfaces;
using LittleSword.Player;
using UnityEngine;

// ============================================================
// Archer: 궁수 직업 클래스
// ============================================================
// BasePlayer를 상속받아서 기본 이동/피격 기능은 그대로 쓰고,
// 궁수만의 특수 공격(화살 발사)을 추가로 구현했어.
//
// 공격 흐름:
//   1. 플레이어가 공격 버튼 누름
//   2. BasePlayer.Attack()이 호출되어 공격 애니메이션 재생
//   3. 애니메이션 특정 프레임에 OnArcherAttackEvent() 호출 (Animation Event)
//   4. FireArrow()로 화살을 실제로 발사!
//
// Animation Event란?
//   유니티 애니메이션 클립의 특정 프레임에서 함수를 자동으로 호출하는 기능이야.
//   활을 당기는 모션이 완료되는 순간에 화살이 나가도록 타이밍을 맞출 수 있어.
// ============================================================
public class Archer : BasePlayer
{
    // 화살 프리팹: 인스펙터에서 화살 게임 오브젝트를 연결해줘야 해
    // 프리팹(Prefab) = 미리 만들어둔 게임 오브젝트 템플릿
    [SerializeField] private GameObject ArrowPrefab;

    // 화살이 발사될 위치 (활의 끝부분에 빈 게임 오브젝트를 만들어서 연결)
    [SerializeField] private Transform firePoint;

    // 애니메이션 이벤트에서 호출되는 함수 (Animation Event에서 이 함수 이름을 지정해야 해)
    public void OnArcherAttackEvent()
    {
        FireArrow(); // 실제 화살 발사 함수 호출
    }

    // 화살 발사 처리
    private void FireArrow()
    {
        // 스프라이트가 좌우 반전됐으면(왼쪽 방향) 화살도 180도 회전해서 발사
        // Quaternion.Euler(x, y, z): 오일러 각도로 회전값 생성
        Quaternion rot = Quaternion.Euler(0, spriteRenderer.flipX ? 180 : 0, 0);

        // Instantiate: 프리팹을 복제해서 게임 씬에 생성
        // 파라미터: (복제할 오브젝트, 생성 위치, 생성 시 회전값)
        GameObject arrow = Instantiate(ArrowPrefab, firePoint.position, rot);

        // 생성된 화살 오브젝트의 Arrow 컴포넌트를 가져와서 초기화
        // Init(힘, 데미지): 화살의 날아가는 속도와 피해량을 설정
        arrow.GetComponent<Arrow>().Init(playerStats.fireForce, playerStats.attackDamage);
    }
}
