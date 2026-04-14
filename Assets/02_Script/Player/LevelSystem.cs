using System;
using UnityEngine;

namespace LittleSword.Player
{
    // ============================================================
    // LevelSystem: 플레이어 경험치 & 레벨업 시스템
    // ============================================================
    // 적을 처치하면 경험치(XP)를 얻고,
    // 일정량의 경험치가 쌓이면 레벨업해서 스탯이 올라가!
    //
    // 레벨업 시 올라가는 스탯:
    //   - maxHP + 10
    //   - attackDamage + 2
    //   - moveSpeed + 0.1
    //
    // 필요 경험치 공식: baseXP + (현재레벨 × growthFactor)
    //   → 레벨이 오를수록 더 많은 경험치가 필요해져
    // ============================================================
    public class LevelSystem : MonoBehaviour
    {
        // ─── 설정값 ──────────────────────────────────────────────
        // 레벨 1→2에 필요한 기본 경험치
        [SerializeField] private int baseXP = 100;

        // 레벨당 필요 경험치 증가량
        [SerializeField] private int growthFactor = 50;

        // 레벨업 시 올라가는 스탯 수치
        [SerializeField] private int hpPerLevel = 10;
        [SerializeField] private int damagePerLevel = 2;
        [SerializeField] private float speedPerLevel = 0.1f;

        // ─── 상태 ────────────────────────────────────────────────
        // 현재 레벨
        public int CurrentLevel { get; private set; } = 1;

        // 현재 경험치
        public int CurrentXP { get; private set; } = 0;

        // 다음 레벨업에 필요한 경험치
        public int XPToNextLevel { get; private set; }

        // ─── 이벤트 ──────────────────────────────────────────────
        // XP가 변경되었을 때 (현재XP, 필요XP)
        public event Action<int, int> OnXPChanged;

        // 레벨업했을 때 (새로운 레벨)
        public event Action<int> OnLevelUp;

        // ─── 플레이어 스탯 참조 ──────────────────────────────────
        // PlayerStats를 런타임에 복사해서 수정 (원본 SO 보호)
        private PlayerStats playerStats;

        private void Start()
        {
            // BasePlayer에서 사용 중인 PlayerStats를 가져옴
            playerStats = GetComponent<BasePlayer>().playerStats;

            // 필요 경험치 초기 계산
            XPToNextLevel = CalculateXPRequired();

            // UI 초기화용 이벤트 발생
            OnXPChanged?.Invoke(CurrentXP, XPToNextLevel);
        }

        // ─── 외부에서 호출하는 함수 ──────────────────────────────

        // 경험치를 획득하는 함수 (적 처치 시 DieState에서 호출)
        public void AddXP(int amount)
        {
            CurrentXP += amount;

            // 경험치가 필요량 이상이면 레벨업
            while (CurrentXP >= XPToNextLevel)
            {
                // 초과 경험치를 다음 레벨로 이월
                CurrentXP -= XPToNextLevel;
                LevelUp();
            }

            OnXPChanged?.Invoke(CurrentXP, XPToNextLevel);
        }

        // ─── 내부 함수 ───────────────────────────────────────────

        // 레벨업 처리
        private void LevelUp()
        {
            CurrentLevel++;

            // 스탯 증가
            playerStats.maxHP += hpPerLevel;
            playerStats.attackDamage += damagePerLevel;
            playerStats.moveSpeed += speedPerLevel;

            // 레벨업하면 체력 전부 회복
            GetComponent<BasePlayer>().Heal(playerStats.maxHP);

            // 다음 레벨업에 필요한 경험치 재계산
            XPToNextLevel = CalculateXPRequired();

            OnLevelUp?.Invoke(CurrentLevel);
        }

        // 현재 레벨에서 다음 레벨까지 필요한 경험치 계산
        // 공식: baseXP + (현재레벨 × growthFactor)
        private int CalculateXPRequired()
        {
            return baseXP + (CurrentLevel * growthFactor);
        }
    }
}
