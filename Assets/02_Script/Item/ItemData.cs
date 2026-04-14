using UnityEngine;

namespace LittleSword.Item
{
    // ============================================================
    // ItemType: 아이템 종류를 구분하는 열거형
    // ============================================================
    public enum ItemType
    {
        HPPotion,   // HP 회복 포션
        Coin,       // 코인 (점수/재화)
        ExpGem      // 경험치 젬
    }

    // ============================================================
    // ItemData: 아이템의 데이터를 저장하는 ScriptableObject
    // ============================================================
    // 각 아이템 종류마다 ItemData 에셋을 만들어서
    // 아이콘, 효과량, 드롭 확률 등을 설정할 수 있어.
    //
    // 유니티 에디터에서: 우클릭 → Create → LittleSword → ItemData
    // ============================================================
    [CreateAssetMenu(fileName = "ItemData", menuName = "LittleSword/ItemData", order = 0)]
    public class ItemData : ScriptableObject
    {
        [Header("Item Info")] // --- 아이템 기본 정보 ---
        // 아이템 이름
        public string itemName = "Item";

        // 아이템 아이콘 (스프라이트)
        public Sprite icon;

        // 아이템 종류 (HP 포션, 코인, 경험치 젬)
        public ItemType itemType;

        [Header("Item Effect")] // --- 아이템 효과 ---
        // 효과 수치 (포션: 회복량, 코인: 점수, 젬: 경험치)
        public int value = 10;

        [Header("Drop Settings")] // --- 드롭 설정 ---
        // 드롭 확률 (0.0 ~ 1.0, 1.0 = 100%)
        [Range(0f, 1f)]
        public float dropChance = 0.5f;
    }
}
