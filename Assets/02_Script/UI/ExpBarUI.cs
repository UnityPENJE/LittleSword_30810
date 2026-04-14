using LittleSword.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LittleSword.UI
{
    // ============================================================
    // ExpBarUI: 화면 HUD에 표시되는 경험치 바 & 레벨 텍스트
    // ============================================================
    // Screen Space Canvas에 경험치 바와 레벨 표시를 담당해.
    // LevelSystem의 이벤트를 구독해서 XP가 변하거나
    // 레벨업할 때 UI를 갱신해.
    //
    // 구조 (Canvas 하위):
    //   ExpBarUI
    //     ├─ ExpBar_Background (Image)
    //     ├─ ExpBar_Fill (Image, Filled)
    //     └─ LevelText (TextMeshProUGUI) : "Lv. 1"
    // ============================================================
    public class ExpBarUI : MonoBehaviour
    {
        // ─── UI 요소 ─────────────────────────────────────────────
        // 경험치 바 채우기 이미지
        [SerializeField] private Image expFillImage;

        // 레벨 텍스트 (예: "Lv. 3")
        [SerializeField] private TextMeshProUGUI levelText;

        // ─── 참조 ────────────────────────────────────────────────
        // 플레이어의 LevelSystem 컴포넌트
        [SerializeField] private LevelSystem levelSystem;

        private void OnEnable()
        {
            if (levelSystem == null) return;

            // LevelSystem 이벤트 구독
            levelSystem.OnXPChanged += UpdateExpBar;
            levelSystem.OnLevelUp += UpdateLevel;
        }

        private void OnDisable()
        {
            if (levelSystem == null) return;

            // 이벤트 구독 해제
            levelSystem.OnXPChanged -= UpdateExpBar;
            levelSystem.OnLevelUp -= UpdateLevel;
        }

        // XP가 변경되었을 때 경험치 바 갱신
        private void UpdateExpBar(int currentXP, int xpToNextLevel)
        {
            if (expFillImage != null)
            {
                expFillImage.fillAmount = (float)currentXP / xpToNextLevel;
            }
        }

        // 레벨업했을 때 레벨 텍스트 갱신
        private void UpdateLevel(int newLevel)
        {
            if (levelText != null)
            {
                levelText.text = $"Lv. {newLevel}";
            }
        }
    }
}
