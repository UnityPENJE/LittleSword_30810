using UnityEngine;
using TMPro;

namespace LittleSword.UI
{
    public class ShopUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI goldText;
        

        private void Start()
        {
            if (CurrencyManager.Instance == null) return;
            CurrencyManager.Instance.OnGoldChanged += UpdateGoldUI;
            UpdateGoldUI(CurrencyManager.Instance.Gold);
        }

        private void OnDisable()
        {
            if (CurrencyManager.Instance != null)
                CurrencyManager.Instance.OnGoldChanged -= UpdateGoldUI;
        }

        private void UpdateGoldUI(int gold) => goldText.text = $"{gold}";


        
    }
}