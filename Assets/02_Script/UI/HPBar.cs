using System;
using UnityEngine;
using UnityEngine.UI;

namespace LittleSword.UI
{
    // ============================================================
    // HPBar: 캐릭터 머리 위에 표시되는 HP 바
    // ============================================================
    // 플레이어나 적의 머리 위에 World Space Canvas로 체력바를 보여줘.
    // IDamageable의 OnHPChanged 이벤트를 구독해서 HP가 바뀔 때마다 갱신해.
    //
    // 구조:
    //   HPBar 오브젝트 (Canvas - World Space)
    //     ├─ Background (Image) : 체력바 배경
    //     └─ Fill (Image, Filled) : 현재 HP 비율만큼 채워지는 이미지
    //
    // 적의 HP 바는 피격 후 일정 시간이 지나면 자동으로 숨겨져.
    // ============================================================
    public class HPBar : MonoBehaviour
    {
        // ─── 설정값 ──────────────────────────────────────────────
        // HP 바 채우기 이미지 (Image.type = Filled로 설정해야 함)
        [SerializeField] private Image fillImage;

        // HP 바가 따라다닐 대상 (플레이어 또는 적의 Transform)
        [SerializeField] private Transform target;

        // 대상 머리 위 오프셋 (Y 방향으로 얼마나 위에 표시할지)
        [SerializeField] private Vector3 offset = new Vector3(0, 1.2f, 0);

        // 적 전용: 피격 후 HP 바가 보이는 시간 (초)
        [SerializeField] private float hideDelay = 3.0f;

        // 플레이어 HP 바는 항상 보이게 할지 여부
        [SerializeField] private bool alwaysVisible = false;

        // ─── 내부 상태 ───────────────────────────────────────────
        // HP 바 부드러운 감소를 위한 현재 fillAmount
        private float currentFill = 1f;

        // 목표 fillAmount (실제 HP 비율)
        private float targetFill = 1f;

        // fillAmount 부드러운 변화 속도
        private float fillSpeed = 5f;

        // 마지막으로 데미지를 받은 시간 (HP 바 숨기기 타이머용)
        private float lastDamageTime;

        // Canvas 그룹 (투명도 조절로 HP 바 숨기기)
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();

            // 항상 보이는 게 아니면 시작할 때 숨겨둠
            if (!alwaysVisible && canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
        }

        private void LateUpdate()
        {
            // HP 바가 대상을 따라다니도록 위치 갱신
            if (target != null)
            {
                transform.position = target.position + offset;
            }

            // fillAmount를 부드럽게 목표값으로 보간
            // Lerp: 현재값에서 목표값으로 서서히 변화 (부드러운 HP 감소 효과)
            currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * fillSpeed);
            fillImage.fillAmount = currentFill;

            // 적 HP 바: 일정 시간 후 자동 숨김
            if (!alwaysVisible && canvasGroup != null)
            {
                if (Time.time - lastDamageTime > hideDelay)
                {
                    // 서서히 투명해지면서 숨겨짐
                    canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0f, Time.deltaTime * 3f);
                }
            }
        }

        // ─── 외부에서 호출하는 초기화/갱신 함수 ─────────────────

        // HP 바 초기 설정 (대상 지정, 항상 보이기 여부)
        public void Init(Transform owner, bool isAlwaysVisible)
        {
            target = owner;
            alwaysVisible = isAlwaysVisible;

            if (alwaysVisible && canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }

        // HP 변경 이벤트에 연결되는 콜백 함수
        // currentHP: 현재 체력, maxHP: 최대 체력
        public void UpdateHP(int currentHP, int maxHP)
        {
            // HP 비율을 계산해서 목표 fillAmount 설정
            targetFill = (float)currentHP / maxHP;

            // 적 HP 바: 피격 시 보이게 하고 타이머 리셋
            if (!alwaysVisible && canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                lastDamageTime = Time.time;
            }
        }
    }
}
