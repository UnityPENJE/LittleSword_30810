using UnityEngine;
using UnityEngine.UI;

namespace LittleSword.UI
{
    public class EnemyHPBar : MonoBehaviour
    {
        [SerializeField] private Image hpFillImage;
        [SerializeField] private float lerpSpeed = 8f;
        [SerializeField] private float hideDelay = 2f;
        [SerializeField] private bool hideWhenFull = true;

        private LittleSword.Enemy.Enemy enemy;   // ← 전체 경로
        private float targetFill = 1f;
        private float hideTime = 0f;

        private void Awake()
        {
            enemy = GetComponentInParent<LittleSword.Enemy.Enemy>();  // ← 전체 경로
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (enemy != null) enemy.OnHPChanged += UpdateHP;
        }

        private void OnDisable()
        {
            if (enemy != null) enemy.OnHPChanged -= UpdateHP;
        }

        private void Update()
        {
            hpFillImage.fillAmount = Mathf.Lerp(hpFillImage.fillAmount, targetFill, Time.deltaTime * lerpSpeed);
            if (Time.time >= hideTime) gameObject.SetActive(false);
        }

        private void UpdateHP(int current, int max)
        {
            targetFill = (float)current / max;
            if (hideWhenFull && current >= max) return;
            gameObject.SetActive(true);
            hideTime = Time.time + hideDelay;
        }
    }
}