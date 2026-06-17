using UnityEngine;
using TMPro;

namespace LittleSword.UI
{
    public class ShopUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI goldText;

        private NetworkPlayer subscribedPlayer;

        private void OnEnable()
        {
            NetworkPlayer.OnLocalPlayerSpawned += OnPlayerSpawned;

            if (NetworkPlayer.LocalPlayer != null)
                OnPlayerSpawned(NetworkPlayer.LocalPlayer);
        }

        private void OnDisable()
        {
            NetworkPlayer.OnLocalPlayerSpawned -= OnPlayerSpawned;
            if (subscribedPlayer != null)
                subscribedPlayer.OnGoldChanged -= UpdateGoldUI;
        }

        private void OnPlayerSpawned(NetworkPlayer player)
        {
            if (subscribedPlayer != null)
                subscribedPlayer.OnGoldChanged -= UpdateGoldUI;

            subscribedPlayer = player;
            subscribedPlayer.OnGoldChanged += UpdateGoldUI;
            UpdateGoldUI(subscribedPlayer.Gold);
        }

        private void UpdateGoldUI(int gold) => goldText.text = $"{gold}";
    }
}
