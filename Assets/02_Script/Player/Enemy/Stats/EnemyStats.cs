using UnityEngine;


namespace Litte.Enemy.Stats
{
    // ============================================================
    // EnemyStats: 적(Enemy)의 능력치(스탯)를 저장하는 ScriptableObject
    // ============================================================
    // PlayerStats와 마찬가지로 데이터 전용 에셋이야.
    // 적의 종류(일반 몬스터, 보스 등)마다 다른 EnemyStats 에셋을 만들어서
    // 같은 Enemy 스크립트를 재사용할 수 있어. (코드 중복 없이 다양한 적 구현 가능!)
    //
    // [Header("...")]: 인스펙터에서 항목들을 그룹으로 묶어 보기 좋게 해줌
    // ============================================================
    [CreateAssetMenu(fileName = "EnemyStats", menuName = "LittleSword/EnemyStats", order = 0)]
    public class EnemyStats : ScriptableObject
    {
        [Header("Enemy Basic Stats")] // --- 기본 스탯 ---
        public int maxHP = 100;       // 최대 체력
        public float moveSpeed = 3f;  // 이동 속도

        [Header("Enemy Detection Stats")] // --- 탐지 관련 스탯 ---
        // 플레이어를 감지하는 주기(초). 숫자가 작을수록 자주 탐지해서 반응이 빠름
        // 하지만 너무 작으면 매 프레임 탐지해서 성능이 나빠질 수 있어!
        public float detecInterval = 0.5f;

        [Header("Enemy Combat Stats")] // --- 전투 관련 스탯 ---
        // 플레이어를 추격하기 시작하는 탐지 범위 (유니티 단위, 약 미터)
        public float chaseDistance = 5f;

        // 공격이 가능한 범위 (이 거리 안에 들어오면 공격 시작)
        public float attackDistance = 1.5f;

        // 한 번 공격할 때 주는 피해량
        public int attackDamage = 10;

        // 공격 쿨다운: 공격 후 다시 공격하기까지 기다리는 시간(초)
        public float attackCooldown = 1f;
    }
}
