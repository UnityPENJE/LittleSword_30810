using TMPro;
using UnityEngine;

namespace LittleSword.UI
{
    // ============================================================
    // DamagePopup: 피격 시 떠오르는 데미지 숫자
    // ============================================================
    // 적이나 플레이어가 피해를 받으면 피해량이 숫자로 떠올라.
    // 위로 떠오르면서 서서히 투명해지다가 자동으로 사라져.
    //
    // 동작 흐름:
    //   1. DamagePopupSpawner가 피격 위치에 이 오브젝트를 생성
    //   2. Init()으로 데미지 숫자 설정
    //   3. 매 프레임 위로 떠오르면서 투명해짐
    //   4. lifetime이 끝나면 자동 삭제
    //
    // TextMeshPro (월드 스페이스)를 사용해서 선명한 텍스트를 보여줘.
    // ============================================================
    public class DamagePopup : MonoBehaviour
    {
        // ─── 설정값 ──────────────────────────────────────────────
        // 위로 떠오르는 속도
        [SerializeField] private float floatSpeed = 1.5f;

        // 텍스트가 사라지기까지의 시간 (초)
        [SerializeField] private float lifetime = 0.8f;

        // ─── 내부 상태 ───────────────────────────────────────────
        // TextMeshPro 컴포넌트 (월드 스페이스 3D 텍스트)
        private TextMeshPro textMesh;

        // 경과 시간 (0 → lifetime까지 증가)
        private float elapsed = 0f;

        // 시작 스케일 (펀치 스케일 효과용)
        private Vector3 startScale;

        private void Awake()
        {
            textMesh = GetComponent<TextMeshPro>();
            startScale = transform.localScale;
        }

        private void Update()
        {
            // 위로 떠오르기 (매 프레임 Y 위치 증가)
            transform.position += Vector3.up * floatSpeed * Time.deltaTime;

            elapsed += Time.deltaTime;

            // 경과 비율 (0 → 1): 시간이 지날수록 1에 가까워짐
            float ratio = elapsed / lifetime;

            // 투명도: 시간이 지날수록 투명해짐 (1 → 0)
            Color color = textMesh.color;
            color.a = Mathf.Lerp(1f, 0f, ratio);
            textMesh.color = color;

            // 스케일: 처음에 살짝 커졌다가 원래 크기로 (펀치 효과)
            // 1 + sin 곡선: 처음에 약간 커졌다가 줄어듦
            float scaleFactor = 1f + Mathf.Sin(ratio * Mathf.PI) * 0.3f;
            transform.localScale = startScale * scaleFactor;

            // lifetime이 끝나면 오브젝트 삭제
            if (elapsed >= lifetime)
            {
                Destroy(gameObject);
            }
        }

        // ─── 외부에서 호출하는 초기화 함수 ───────────────────────

        // 데미지 숫자와 크리티컬 여부를 설정
        public void Init(int damage, bool isCritical = false)
        {
            textMesh.text = damage.ToString();

            // 크리티컬이면 빨간색 + 큰 폰트, 아니면 노란색 + 기본 폰트
            if (isCritical)
            {
                textMesh.color = Color.red;
                textMesh.fontSize = 8f;
            }
            else
            {
                textMesh.color = Color.yellow;
                textMesh.fontSize = 5f;
            }
        }
    }
}
