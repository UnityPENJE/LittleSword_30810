using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace LittleSword.Player
{
    public class DashSkill : MonoBehaviour
    {
        [Header("대쉬 설정")]
        [SerializeField] private float dashSpeed = 15f;
        [SerializeField] private float dashDuration = 0.15f;
        [SerializeField] private float cooldown = 1f;

        [Header("잔상 설정")]
        [SerializeField] private int afterImageCount = 5;      // 잔상 개수
        [SerializeField] private float afterImageInterval = 0.03f; // 잔상 생성 간격
        [SerializeField] private float afterImageFadeTime = 0.3f;  // 잔상 사라지는 시간
        [SerializeField] private Color afterImageColor = new Color(1, 1, 1, 0.5f);

        [Header("쿨타임 UI")]
        [SerializeField] private Image cooldownImage;

        private bool isCooldown;
        private BasePlayer player;
        private Rigidbody2D rb;
        private SpriteRenderer sr;

        private void Awake()
        {
            player = GetComponentInParent<BasePlayer>();
            rb = GetComponentInParent<Rigidbody2D>();
            sr = GetComponentInParent<SpriteRenderer>();
            if (cooldownImage) cooldownImage.fillAmount = 0f;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) && !isCooldown)
                StartCoroutine(Dash());
        }

        private IEnumerator Dash()
        {
            isCooldown = true;
            player.IsInvincible = true;
            player.CanMove = false;

            Vector2 dir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
            if (dir == Vector2.zero) dir = sr.flipX ? Vector2.left : Vector2.right;

            StartCoroutine(SpawnAfterImages());

            float elapsed = 0f;
            while (elapsed < dashDuration)
            {
                elapsed += Time.deltaTime;
                rb.linearVelocity = dir * dashSpeed;
                yield return null;
            }

            rb.linearVelocity = Vector2.zero; // ← 추가
            player.IsInvincible = false;
            player.CanMove = true;

            // 쿨타임 UI
            elapsed = 0f;
            while (elapsed < cooldown)
            {
                elapsed += Time.deltaTime;
                if (cooldownImage) cooldownImage.fillAmount = 1f - (elapsed / cooldown);
                yield return null;
            }

            if (cooldownImage) cooldownImage.fillAmount = 0f;
            isCooldown = false;
        }

        private IEnumerator SpawnAfterImages()
        {
            for (int i = 0; i < afterImageCount; i++)
            {
                CreateAfterImage();
                yield return new WaitForSeconds(afterImageInterval);
            }
        }

        private void CreateAfterImage()
        {
            // 스프라이트 복사해서 잔상 생성
            GameObject obj = new GameObject("AfterImage");
            obj.transform.position = player.transform.position;
            obj.transform.rotation = transform.rotation;
            obj.transform.localScale = transform.localScale;

            SpriteRenderer ghost = obj.AddComponent<SpriteRenderer>();
            ghost.sprite = sr.sprite;
            ghost.flipX = sr.flipX;
            ghost.color = afterImageColor;
            ghost.sortingLayerName = sr.sortingLayerName;
            ghost.sortingOrder = sr.sortingOrder - 1;

            StartCoroutine(FadeAfterImage(ghost));
        }

        private IEnumerator FadeAfterImage(SpriteRenderer ghost)
        {
            float elapsed = 0f;
            Color c = ghost.color;
            while (elapsed < afterImageFadeTime)
            {
                elapsed += Time.deltaTime;
                c.a = Mathf.Lerp(afterImageColor.a, 0f, elapsed / afterImageFadeTime);
                ghost.color = c;
                yield return null;
            }
            Destroy(ghost.gameObject);
        }
    }
}