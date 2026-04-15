using UnityEngine;
using UnityEngine.UI;
using LittleSword.Player;

namespace LittleSword.UI
{
    // ============================================================
    // PlayerHPBarUI: 화면에 고정된 플레이어 HP 바
    // ============================================================
    // Canvas(Screen Space Overlay) 아래 Image 두 개로 구성:
    //   - 배경 Image (회색)
    //   - Fill Image (빨강, Image Type = Filled, Fill Method = Horizontal)
    // ============================================================
    public class PlayerHPBarUI : MonoBehaviour
    {
        [SerializeField] private BasePlayer player;     // 추적할 플레이어
        [SerializeField] private Image hpFillImage;     // fillAmount로 HP 표시
        [SerializeField] private float lerpSpeed = 5f;  // HP 감소 애니메이션 속도

        private float targetFill = 1f; // 목표 fillAmount (실제 HP 비율)

        private void OnEnable()
        {
            if (player != null) player.OnHPChanged += UpdateHP;
        }

        private void OnDisable()
        {
            if (player != null) player.OnHPChanged -= UpdateHP;
        }

        private void Update()
        {
            // 부드럽게 감소
            hpFillImage.fillAmount = Mathf.Lerp(hpFillImage.fillAmount, targetFill, Time.deltaTime * lerpSpeed);
        }

        private void UpdateHP(int current, int max)
        {
            targetFill = (float)current / max;
        }
    }
}