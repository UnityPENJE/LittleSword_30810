namespace LittleSword.Network
{
    // ============================================================
    // PlayerClassType: 선택 가능한 직업 종류를 정의하는 열거형(enum)
    // ============================================================
    // enum = 이름 있는 정수 상수들의 집합
    // 숫자 대신 의미 있는 이름으로 직업을 구분할 수 있어.
    //
    // 사용 예:
    //   PlayerClassType myClass = PlayerClassType.Warrior;
    //   if (myClass == PlayerClassType.Healer) { ... }
    // ============================================================
    public enum PlayerClassType
    {
        Warrior = 0,  // 근접 전사 (기존)
        Archer  = 1,  // 원거리 궁수 (기존)
        Lancer  = 2,  // 창병 - 전방 찌르기 (신규)
        Healer  = 3,  // 힐러 - 아군 회복 (신규)
    }
}
