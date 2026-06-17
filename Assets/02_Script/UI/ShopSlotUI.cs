using UnityEngine;
using UnityEngine.UI;
using LittleSword.Player;

namespace LittleSword.UI
{
    public class ShopSlotUI : MonoBehaviour
    {
        [SerializeField] private string itemKey;
        [SerializeField] private int price;
        [SerializeField] private Button buyButton;

        // 런타임에 로컬 플레이어에서 찾아서 채워짐 (Inspector 할당 불필요)
        private GameObject skillObject;

        private string SaveKey => $"{NetworkPlayer.LocalPlayer?.OwnerClientId ?? 0}_{itemKey}";

        private void Start()
        {
            buyButton.onClick.AddListener(OnBuy);
            NetworkPlayer.OnLocalPlayerSpawned += OnLocalPlayerSpawned;

            // 이미 스폰돼 있으면 즉시 처리 (씬 재진입 등)
            if (NetworkPlayer.LocalPlayer != null)
                OnLocalPlayerSpawned(NetworkPlayer.LocalPlayer);
        }

        private void OnDestroy()
        {
            NetworkPlayer.OnLocalPlayerSpawned -= OnLocalPlayerSpawned;
        }

        private void OnLocalPlayerSpawned(NetworkPlayer networkPlayer)
        {
            skillObject = FindSkillObject(networkPlayer);

            bool purchased = PlayerPrefs.GetInt(SaveKey, 0) == 1;
            if (purchased)
            {
                if (skillObject != null) skillObject.SetActive(true);
                buyButton.interactable = false;
            }
        }

        // itemKey로 플레이어 프리팹에서 해당 스킬 컴포넌트의 GameObject를 찾음
        private GameObject FindSkillObject(NetworkPlayer networkPlayer)
        {
            var go = networkPlayer.gameObject;
            return itemKey.ToLower() switch
            {
                "dash"  => go.GetComponentInChildren<DashSkill>(true)?.gameObject,
                "heal"  => go.GetComponentInChildren<HealSkill>(true)?.gameObject,
                "parry" => go.GetComponentInChildren<ParrySkill>(true)?.gameObject,
                _ => null
            };
        }

        private void OnBuy()
        {
            if (PlayerPrefs.GetInt(SaveKey, 0) == 1) return;

            var localPlayer = NetworkPlayer.LocalPlayer;
            if (localPlayer == null) return;
            if (localPlayer.Gold < price) return;

            localPlayer.SpendGoldServerRpc(price);

            PlayerPrefs.SetInt(SaveKey, 1);
            if (skillObject != null) skillObject.SetActive(true);
            buyButton.interactable = false;
        }
    }
}
