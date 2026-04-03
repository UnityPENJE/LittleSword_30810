using LittleSword.Interfaces;
using LittleSword.Player;
using UnityEngine;

public class Archer : BasePlayer
{
    [SerializeField] private GameObject ArrowPrefab;
    [SerializeField] private Transform firePoint;


    // 애니메이터 이벤트에서 호출됨 = 공격 애니메이션 타이밍에 맞춰 실행
    public void OnArcherAttackEvent()
    {
        FireArrow();
    }

    // 화살 생성 / 초기화
    private void FireArrow()
    {
        // 스프라이트 반향에 따라 방향 보정
        Quaternion rot = Quaternion.Euler(0, spriteRenderer.flipX ? 180 : 0, 0);

        // 화살 프리팹을 발사 위치에 인스턴스화
        GameObject arrow = Instantiate(ArrowPrefab, firePoint.position, rot);

        // 생성된 화살 초기화: 발사력과 데미지를 플레이어 스탯에서 전달
        arrow.GetComponent<Arrow>().Init(playerStats.fireForce, playerStats.attackDamage);
    }
}
