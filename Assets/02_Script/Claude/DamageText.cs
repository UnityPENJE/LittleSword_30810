using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace LittleSword.UI
{
    public class DamageText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private float moveSpeed = 1.5f;   // 위로 올라가는 속도
        [SerializeField] private float duration = 0.8f;    // 사라지기까지 시간
        [SerializeField] private Color color = Color.red;

        public void Show(int damage)
        {
            text.text = damage.ToString();
            text.color = color;
            StartCoroutine(Animate());
        }

        private IEnumerator Animate()
        {
            float elapsed = 0f;
            Vector3 startPos = transform.position;
            Color startColor = text.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // 위로 이동
                transform.position = startPos + Vector3.up * (moveSpeed * t);

                // 점점 투명해짐
                text.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}