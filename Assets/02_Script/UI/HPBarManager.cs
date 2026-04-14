using LittleSword.Player;
using UnityEngine;

namespace LittleSword.UI
{
    // ============================================================
    // HPBarManager: HP 바 생성 및 관리를 담당하는 매니저
    // ============================================================
    // 씬에 존재하는 싱글톤으로, 플레이어나 적이 생성될 때
    // HP 바 프리팹을 인스턴스화해서 머리 위에 붙여줘.
    //
    // 사용 방법 (다른 스크립트에서):
    //   HPBarManager.Instance.CreateHPBar(transform, OnHPChanged, true);
    //
    // 플레이어는 alwaysVisible = true (항상 보임)
    // 적은 alwaysVisible = false (피격 시에만 보임)
    // ============================================================
    public class HPBarManager : MonoBehaviour
    {
        // ─── 싱글톤 ──────────────────────────────────────────────
        public static HPBarManager Instance { get; private set; }

        // ─── 설정값 ──────────────────────────────────────────────
        // HP 바 프리팹 (Canvas + Image로 구성된 프리팹을 인스펙터에서 연결)
        [SerializeField] private GameObject hpBarPrefab;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        // HP 바를 생성하고 대상에게 연결하는 함수
        // owner: HP 바가 따라다닐 대상 Transform
        // onHPChanged: HP 변경 이벤트에 연결 (ref로 받아서 콜백 등록)
        // alwaysVisible: true면 항상 보임 (플레이어용)
        public HPBar CreateHPBar(Transform owner, bool alwaysVisible)
        {
            if (hpBarPrefab == null) return null;

            // HP 바 프리팹을 씬에 생성
            GameObject hpBarObj = Instantiate(hpBarPrefab, owner.position, Quaternion.identity);

            // HPBar 컴포넌트를 가져와서 초기화
            HPBar hpBar = hpBarObj.GetComponent<HPBar>();
            hpBar.Init(owner, alwaysVisible);

            return hpBar;
        }
    }
}
