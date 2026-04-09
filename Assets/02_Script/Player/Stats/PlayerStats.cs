using UnityEngine;

// ============================================================
// PlayerStats: 플레이어의 능력치(스탯)를 저장하는 ScriptableObject
// ============================================================
// ScriptableObject란?
//   유니티에서 데이터만 저장하기 위한 특별한 클래스야.
//   MonoBehaviour처럼 게임 오브젝트에 붙이는 게 아니라,
//   프로젝트 창에 에셋(파일)으로 저장해두고 여러 곳에서 공유해서 쓸 수 있어.
//
// [CreateAssetMenu]: 유니티 에디터 메뉴에서 우클릭 → Create → LittleSword → PlayerStats
//   로 이 ScriptableObject 에셋을 생성할 수 있게 해줌
// ============================================================
[CreateAssetMenu(fileName = "PlayerStatsSO", menuName = "LittleSword/PlayerStats", order = 0)]
public class PlayerStats : ScriptableObject
{
    // 플레이어의 최대 체력 (기본값 100)
    public int maxHP = 100;

    // 플레이어의 이동 속도 (기본값 5)
    public float moveSpeed = 5f;

    // 근접 공격 데미지 (적에게 주는 피해량)
    public int attackDamage = 20;

    // 화살 발사 힘 (Archer 전용 - 숫자가 클수록 화살이 빠르게 날아감)
    public float fireForce = 10f;

    // 공격 쿨다운: 공격 후 다시 공격할 수 있게 되기까지 걸리는 시간(초)
    public float Attackcooldwon;
}
