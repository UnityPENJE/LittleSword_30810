using UnityEngine;
using UnityEngine.UI;

namespace LittleSword.UI
{
    // 플레이어 프리팹 안에 있는 HP 바 UI
    // 같은 프리팹의 NetworkPlayer에서 HP 변경 이벤트를 직접 받음
    public class PlayerHPBarUI : MonoBehaviour
    {
        [SerializeField] private Image hpFillImage;
        [SerializeField] private float lerpSpeed = 5f;

        private float targetFill = 1f;
        private NetworkPlayer networkPlayer;

        private void Awake()
        {
            networkPlayer = GetComponentInParent<NetworkPlayer>();
        }

        private void OnEnable()
        {
            if (networkPlayer != null)
                networkPlayer.OnHPChanged += UpdateHP;
        }

        private void OnDisable()
        {
            if (networkPlayer != null)
                networkPlayer.OnHPChanged -= UpdateHP;
        }

        private void Update()
        {
            if (hpFillImage != null)
                hpFillImage.fillAmount = Mathf.Lerp(hpFillImage.fillAmount, targetFill, Time.deltaTime * lerpSpeed);
        }

        private void UpdateHP(int current, int max)
        {
            targetFill = (float)current / max;
        }
    }
}
