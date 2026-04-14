using System;
using UnityEngine;

namespace LittleSword.InputSystem
{
    // ============================================================
    // IInputEvents: 입력 이벤트를 정의하는 인터페이스
    // ============================================================
    // 인터페이스(interface)란? → "이 기능들은 반드시 구현해야 해!" 라는 약속표야.
    // 예를 들어 InputHandler 클래스가 이 인터페이스를 구현하면,
    // 반드시 OnMove와 OnAttack 이벤트를 가지게 돼.
    //
    // 왜 인터페이스를 쓸까?
    // → 나중에 키보드 대신 게임패드나 다른 입력 방식으로 바꿔도,
    //   이 인터페이스만 지키면 나머지 코드를 고치지 않아도 돼!
    // ============================================================
    public interface IInputEvents
    {
        // 이동 입력 이벤트: 플레이어가 방향키/조이스틱을 움직이면 호출됨
        // Vector2 = (x, y) 방향값을 전달해 (예: 오른쪽이면 (1, 0))
        event Action<Vector2> OnMove;

        // 공격 입력 이벤트: 플레이어가 공격 버튼을 누르면 호출됨
        event Action OnAttack;

        // 대시 입력 이벤트: 플레이어가 대시(회피) 버튼을 누르면 호출됨
        event Action OnDash;

        // 스킬 입력 이벤트: 플레이어가 스킬 버튼을 누르면 호출됨
        event Action OnSkill;
    }
}
