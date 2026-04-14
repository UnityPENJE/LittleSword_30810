using LittleSword.Player;
using UnityEngine;

namespace LittleSword.Item
{
    // ============================================================
    // DroppedItem: 바닥에 떨어진 아이템 오브젝트
    // ============================================================
    // 적이 죽으면 이 프리팹이 생성되어 바닥에 떨어져.
    // 플레이어가 가까이 오면 자석처럼 끌려오고,
    // 플레이어와 접촉하면 효과를 적용하고 사라져.
    //
    // 아이템 종류에 따른 효과:
    //   HPPotion → 체력 회복
    //   Coin → 점수 추가 (현재는 로그 출력)
    //   ExpGem → 경험치 획득
    //
    // 필요 컴포넌트: SpriteRenderer, CircleCollider2D (isTrigger)
    // ============================================================
    public class DroppedItem : MonoBehaviour
    {
        // ─── 설정값 ──────────────────────────────────────────────
        // 아이템 데이터 (ScriptableObject)
        [SerializeField] private ItemData itemData;

        // 자석 끌림 범위 (이 거리 안에 플레이어가 오면 끌려감)
        [SerializeField] private float magnetRange = 2.0f;

        // 자석 끌림 속도
        [SerializeField] private float magnetSpeed = 8.0f;

        // 아이템 자동 삭제 시간 (초)
        [SerializeField] private float despawnTime = 15.0f;

        // ─── 내부 상태 ───────────────────────────────────────────
        private SpriteRenderer spriteRenderer;
        private Transform playerTransform;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            // 아이콘 스프라이트 적용
            if (itemData != null && itemData.icon != null)
            {
                spriteRenderer.sprite = itemData.icon;
            }

            // 일정 시간 후 자동 삭제 (바닥에 영원히 남아있는 것 방지)
            Destroy(gameObject, despawnTime);

            // 씬에서 플레이어를 찾아둠
            BasePlayer player = FindObjectOfType<BasePlayer>();
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        private void Update()
        {
            // 플레이어가 자석 범위 안에 있으면 끌려감
            if (playerTransform == null) return;

            float distance = Vector2.Distance(transform.position, playerTransform.position);

            if (distance < magnetRange)
            {
                // MoveTowards: 현재 위치에서 목표 위치로 일정 속도로 이동
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    playerTransform.position,
                    magnetSpeed * Time.deltaTime
                );
            }
        }

        // 플레이어와 접촉했을 때 아이템 효과 적용
        // 트리거 콜라이더가 겹치면 호출됨
        private void OnTriggerEnter2D(Collider2D other)
        {
            // "Player" 태그가 아니면 무시
            if (!other.CompareTag("Player")) return;

            BasePlayer player = other.GetComponent<BasePlayer>();
            if (player == null) return;

            // 아이템 종류에 따라 효과 적용
            ApplyEffect(player);

            // 아이템 오브젝트 삭제
            Destroy(gameObject);
        }

        // 아이템 종류에 따른 효과 적용
        private void ApplyEffect(BasePlayer player)
        {
            switch (itemData.itemType)
            {
                case ItemType.HPPotion:
                    // HP 회복
                    player.Heal(itemData.value);
                    break;

                case ItemType.Coin:
                    // 코인 획득 (추후 점수 시스템 연동)
                    Debug.Log($"코인 획득: +{itemData.value}");
                    break;

                case ItemType.ExpGem:
                    // 경험치 획득
                    LevelSystem levelSystem = player.GetComponent<LevelSystem>();
                    levelSystem?.AddXP(itemData.value);
                    break;
            }
        }

        // ─── 외부에서 호출하는 초기화 함수 ───────────────────────

        // ItemDropTable에서 아이템을 생성할 때 ItemData를 설정
        public void Init(ItemData data)
        {
            itemData = data;

            // 아이콘 스프라이트 즉시 적용
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            if (data.icon != null)
            {
                spriteRenderer.sprite = data.icon;
            }
        }
    }
}
