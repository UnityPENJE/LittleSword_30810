using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using LittleSword.Effects;
using Unity.Netcode;

namespace LittleSword.Player
{
    public class ParrySkill : MonoBehaviour
    {
        [Header("�и� ����")]
        [SerializeField] private GameObject parryEffect;
        [SerializeField] private float parryDuration = 0.4f;
        [SerializeField] private float cooldown = 2f;

        [Header("��Ÿ�� UI")]
        [SerializeField] private Image cooldownImage;

        [Header("�и� ���� ȿ��")]
        [SerializeField] private Image flashImage;  // ��üȭ�� ��� Image (���� 0���� ����)
        [SerializeField] private float flashDuration = 0.2f;

        private bool isCooldown;
        private BasePlayer player;
        private NetworkPlayer networkPlayer;

        private void Awake()
        {
            player = GetComponentInParent<BasePlayer>();
            networkPlayer = GetComponentInParent<NetworkPlayer>();
            parryEffect.SetActive(false);
            if (cooldownImage) cooldownImage.fillAmount = 0f;
            if (flashImage) flashImage.color = new Color(1, 1, 1, 0);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(1) && !isCooldown)
                StartCoroutine(Parry());
        }

        private IEnumerator Parry()
        {
            isCooldown = true;
            parryEffect.SetActive(true);
            // 멀티: 서버에도 무적 상태 동기화, 싱글: 직접 설정
            if (networkPlayer != null)
                networkPlayer.SetInvincibleServerRpc(true);
            else
                player.IsInvincible = true;

            yield return new WaitForSeconds(parryDuration);

            parryEffect.SetActive(false);
            if (networkPlayer != null)
                networkPlayer.SetInvincibleServerRpc(false);
            else
                player.IsInvincible = false;

            float elapsed = 0f;
            while (elapsed < cooldown)
            {
                elapsed += Time.deltaTime;
                if (cooldownImage) cooldownImage.fillAmount = 1f - (elapsed / cooldown);
                yield return null;
            }

            if (cooldownImage) cooldownImage.fillAmount = 0f;
            isCooldown = false;
        }

        // BasePlayer.TakeDamage���� ���� �� ������ ������ ȣ��
        public void OnParrySuccess()
        {
            CameraShake.Instance?.Shake();
            StartCoroutine(Flash());
        }

        private IEnumerator Flash()
        {
            float elapsed = 0f;
            while (elapsed < flashDuration)
            {
                elapsed += Time.deltaTime;
                float a = 1f - (elapsed / flashDuration);
                if (flashImage) flashImage.color = new Color(1, 1, 1, a);
                yield return null;
            }
            if (flashImage) flashImage.color = new Color(1, 1, 1, 0);
        }
    }
}