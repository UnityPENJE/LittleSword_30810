using UnityEngine;
using UnityEngine.UI;

namespace LittleSword.UI
{
    public class ShopSlotUI : MonoBehaviour
    {
        [SerializeField] private string itemKey;
        [SerializeField] private int price;
        [SerializeField] private GameObject skillObject;
        [SerializeField] private Button buyButton;

        // 클라이언트마다 별도 구매 기록 (같은 PC에서 테스트할 때도 분리)
        private string SaveKey => $"{NetworkPlayer.LocalPlayer?.OwnerClientId ?? 0}_{itemKey}";

        private void Start()
        {
            bool purchased = PlayerPrefs.GetInt(SaveKey, 0) == 1;
            if (purchased)
            {
                skillObject?.SetActive(true);
                buyButton.interactable = false;
            }
            buyButton.onClick.AddListener(OnBuy);
        }

        private void OnBuy()
        {
            if (PlayerPrefs.GetInt(SaveKey, 0) == 1) return;

            var localPlayer = NetworkPlayer.LocalPlayer;
            if (localPlayer == null) return;
            if (localPlayer.Gold < price) return;

            // 서버에 골드 차감 요청
            localPlayer.SpendGoldServerRpc(price);

            PlayerPrefs.SetInt(SaveKey, 1);
            skillObject?.SetActive(true);
            buyButton.interactable = false;
        }
    }
}
