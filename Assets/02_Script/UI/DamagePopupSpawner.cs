using UnityEngine;

namespace LittleSword.UI
{
    // ============================================================
    // DamagePopupSpawner: 데미지 팝업을 생성하는 매니저
    // ============================================================
    // 싱글톤 패턴으로, 어디서든 DamagePopupSpawner.Instance.Spawn()으로
    // 데미지 팝업을 생성할 수 있어.
    //
    // 사용 예시:
    //   DamagePopupSpawner.Instance.Spawn(transform.position, 25);
    //
    // 팝업은 약간의 랜덤 X 오프셋으로 생성되어
    // 여러 데미지가 겹치지 않고 자연스럽게 퍼져 보여.
    // ============================================================
    public class DamagePopupSpawner : MonoBehaviour
    {
        // ─── 싱글톤 ──────────────────────────────────────────────
        public static DamagePopupSpawner Instance { get; private set; }

        // ─── 설정값 ──────────────────────────────────────────────
        // 데미지 팝업 프리팹 (TextMeshPro + DamagePopup 스크립트가 붙어있음)
        [SerializeField] private GameObject popupPrefab;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        // 지정된 위치에 데미지 팝업 생성
        // position: 팝업이 나타날 월드 좌표
        // damage: 표시할 데미지 숫자
        // isCritical: 크리티컬 히트 여부 (큰 빨간 글씨)
        public void Spawn(Vector3 position, int damage, bool isCritical = false)
        {
            if (popupPrefab == null) return;

            // 약간의 랜덤 X 오프셋을 줘서 여러 팝업이 겹치지 않게 함
            Vector3 spawnPos = position + new Vector3(Random.Range(-0.3f, 0.3f), 0.5f, 0f);

            // 프리팹 인스턴스 생성
            GameObject popupObj = Instantiate(popupPrefab, spawnPos, Quaternion.identity);

            // DamagePopup 컴포넌트에 데미지 숫자 전달
            popupObj.GetComponent<DamagePopup>().Init(damage, isCritical);
        }
    }
}
