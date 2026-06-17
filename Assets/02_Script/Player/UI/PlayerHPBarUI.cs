using UnityEngine;
using UnityEngine.UI;
using LittleSword.Player;

namespace LittleSword.UI
{
    // ============================================================
    // PlayerHPBarUI: ȭ�鿡 ������ �÷��̾� HP ��
    // ============================================================
    // Canvas(Screen Space Overlay) �Ʒ� Image �� ���� ����:
    //   - ��� Image (ȸ��)
    //   - Fill Image (����, Image Type = Filled, Fill Method = Horizontal)
    // ============================================================
    public class PlayerHPBarUI : MonoBehaviour
    {
        [SerializeField] private BasePlayer player;
        [SerializeField] private Image hpFillImage;
        [SerializeField] private float lerpSpeed = 5f;

        private float targetFill = 1f;

        // NetworkPlayer.OnNetworkSpawn()에서 호출해 로컬 플레이어 연결
        public void SetPlayer(BasePlayer newPlayer)
        {
            if (player != null) player.OnHPChanged -= UpdateHP;
            player = newPlayer;
            if (player != null)
            {
                player.OnHPChanged += UpdateHP;
                UpdateHP(player.CurrentHP, player.playerStats.maxHP);
            }
        }

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
            hpFillImage.fillAmount = Mathf.Lerp(hpFillImage.fillAmount, targetFill, Time.deltaTime * lerpSpeed);
        }

        private void UpdateHP(int current, int max)
        {
            targetFill = (float)current / max;
        }
    }
}