using System.Collections.Generic;
using UnityEngine;

namespace LittleSword.Item
{
    // ============================================================
    // ItemDropTable: 적 처치 시 아이템 드롭을 관리하는 클래스
    // ============================================================
    // Enemy 게임 오브젝트에 붙여서 사용해.
    // 적이 죽을 때 이 클래스의 SpawnDrops()를 호출하면,
    // possibleDrops 리스트에서 확률에 따라 아이템을 생성해.
    //
    // 사용 방법:
    //   1. Enemy 프리팹에 ItemDropTable 컴포넌트 추가
    //   2. possibleDrops에 ItemData 에셋들을 등록
    //   3. droppedItemPrefab에 DroppedItem 프리팹 연결
    //   4. DieState에서 SpawnDrops() 호출
    // ============================================================
    public class ItemDropTable : MonoBehaviour
    {
        // ─── 설정값 ──────────────────────────────────────────────
        // 드롭 가능한 아이템 목록 (인스펙터에서 ItemData 에셋들을 등록)
        [SerializeField] private List<ItemData> possibleDrops;

        // 드롭된 아이템 프리팹 (DroppedItem 스크립트가 붙어있는 프리팹)
        [SerializeField] private GameObject droppedItemPrefab;

        // 아이템이 흩어지는 범위 (적 위치 기준으로 이 범위 안에 랜덤 배치)
        [SerializeField] private float scatterRange = 0.5f;

        // ─── 외부에서 호출하는 함수 ──────────────────────────────

        // 적 사망 위치에 아이템을 드롭하는 함수
        // position: 아이템이 떨어질 위치 (적의 transform.position)
        public void SpawnDrops(Vector3 position)
        {
            if (possibleDrops == null || droppedItemPrefab == null) return;

            // 각 아이템에 대해 드롭 확률을 굴림
            foreach (ItemData itemData in possibleDrops)
            {
                // Random.value: 0.0 ~ 1.0 사이의 랜덤 값
                // dropChance 이하면 드롭 성공!
                if (Random.value <= itemData.dropChance)
                {
                    SpawnItem(position, itemData);
                }
            }
        }

        // 개별 아이템을 씬에 생성하는 함수
        private void SpawnItem(Vector3 position, ItemData itemData)
        {
            // 적 위치에서 약간 흩어진 랜덤 위치에 생성
            // insideUnitCircle: 반지름 1인 원 안의 랜덤 좌표
            Vector2 offset = Random.insideUnitCircle * scatterRange;
            Vector3 spawnPos = position + new Vector3(offset.x, offset.y, 0f);

            // 프리팹 인스턴스 생성
            GameObject itemObj = Instantiate(droppedItemPrefab, spawnPos, Quaternion.identity);

            // DroppedItem 컴포넌트에 아이템 데이터 전달
            DroppedItem droppedItem = itemObj.GetComponent<DroppedItem>();
            droppedItem?.Init(itemData);
        }
    }
}
