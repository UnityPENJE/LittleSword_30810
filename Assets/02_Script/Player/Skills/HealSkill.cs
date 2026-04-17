using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace LittleSword.Player
{
    public class HealSkill : MonoBehaviour
    {
        [Header("회복 설정")]
        [SerializeField] private int healAmount = 30;
        [SerializeField] private float cooldown = 10f;
        [SerializeField] private KeyCode healKey = KeyCode.E;

        [Header("쿨타임 UI")]
        [SerializeField] private Image cooldownImage;

        private bool isCooldown;
        private BasePlayer player;

        private void Awake()
        {
            player = GetComponentInParent<BasePlayer>();
            if (cooldownImage) cooldownImage.fillAmount = 0f;
        }

        private void Update()
        {
            if (Input.GetKeyDown(healKey) && !isCooldown)
                StartCoroutine(Heal());
        }

        private IEnumerator Heal()
        {
            isCooldown = true;
            player.Heal(healAmount);

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
    }
}