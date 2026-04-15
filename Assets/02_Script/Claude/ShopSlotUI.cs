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

        private void Start()
        {
            bool purchased = PlayerPrefs.GetInt(itemKey, 0) == 1;
            if (purchased) skillObject?.SetActive(true);
            buyButton.onClick.AddListener(OnBuy);
        }

        private void OnBuy()
        {
            if (PlayerPrefs.GetInt(itemKey, 0) == 1) return;
            if (!CurrencyManager.Instance.Spend(price)) return;

            PlayerPrefs.SetInt(itemKey, 1);
            skillObject?.SetActive(true);
            buyButton.interactable = false;
        }
    }
}