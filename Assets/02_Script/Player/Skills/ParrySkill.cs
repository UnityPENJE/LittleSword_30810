using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using LittleSword.Effects;

namespace LittleSword.Player
{
    public class ParrySkill : MonoBehaviour
    {
        [Header("패링 설정")]
        [SerializeField] private GameObject parryEffect;
        [SerializeField] private float parryDuration = 0.4f;
        [SerializeField] private float cooldown = 2f;

        [Header("쿨타임 UI")]
        [SerializeField] private Image cooldownImage;

        [Header("패링 성공 효과")]
        [SerializeField] private Image flashImage;  // 전체화면 흰색 Image (알파 0으로 시작)
        [SerializeField] private float flashDuration = 0.2f;

        private bool isCooldown;
        private BasePlayer player;

        private void Awake()
        {
            player = GetComponentInParent<BasePlayer>();
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
            player.IsInvincible = true;

            yield return new WaitForSeconds(parryDuration);

            parryEffect.SetActive(false);
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

        // BasePlayer.TakeDamage에서 무적 중 데미지 받으면 호출
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