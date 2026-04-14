using LittleSword.InputSystem;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using Logger = LittleSword.Common.Logger;

namespace LittleSword.InputSystem
{
    // ============================================================
    // InputHandler: 키보드/게임패드 입력을 받아서 이벤트로 전달하는 클래스
    // ============================================================
    // 유니티의 새 Input System을 사용해서 입력을 처리해.
    // 핵심 아이디어: InputHandler는 입력을 받고, 이벤트(event)를 통해
    // 관심 있는 다른 클래스(예: BasePlayer)에게 "이 입력이 들어왔어!" 라고 알려줘.
    //
    // MonoBehaviour = 유니티 게임 오브젝트에 붙일 수 있는 스크립트의 기본 클래스
    // IInputEvents = 이 클래스가 OnMove, OnAttack 이벤트를 반드시 구현함을 보장
    // ============================================================
    public class InputHandler : MonoBehaviour, IInputEvents
    {
        // 외부에서 구독할 수 있는 이벤트들
        // event: 이 클래스 밖에서는 구독(+=)만 할 수 있고, 직접 발동(Invoke)은 못해
        public event Action<Vector2> OnMove;    // 이동 입력 이벤트
        public event Action OnAttack;           // 공격 입력 이벤트
        public event Action OnDash;             // 대시(회피) 입력 이벤트
        public event Action OnSkill;            // 스킬 입력 이벤트

        // 유니티 Input System이 자동 생성한 액션 클래스 (InputSystem_Actions.cs)
        private InputSystem_Actions inputActions;

        // 이동 액션 (WASD, 방향키, 좌측 조이스틱 등)
        private InputAction moveAction;

        // 공격 액션 (마우스 클릭, 특정 버튼 등)
        private InputAction attackAction;

        // 대시(회피) 액션 (스페이스바)
        private InputAction dashAction;

        // 스킬 액션 (Q키 또는 Interact)
        private InputAction skillAction;

        // Awake: 게임 오브젝트가 처음 생성될 때 딱 한 번 호출됨 (Start보다 먼저!)
        private void Awake()
        {
            // Input System 액션 클래스 인스턴스 생성
            inputActions = new InputSystem_Actions();

            // 플레이어 액션 맵에서 이동/공격 액션을 가져옴
            moveAction = inputActions.Player.Move;
            attackAction = inputActions.Player.Attack;

            // Jump(스페이스바)를 대시로 사용, Interact를 스킬로 사용
            dashAction = inputActions.Player.Jump;
            skillAction = inputActions.Player.Interact;
        }

        // OnEnable: 이 컴포넌트가 활성화될 때 호출됨 (게임 오브젝트가 켜질 때 등)
        private void OnEnable()
        {
            inputActions.Enable(); // 입력 감지 시작

            // 이동 입력이 들어올 때(performed)와 멈출 때(canceled) HandleMove 실행
            moveAction.performed += HandleMove;
            moveAction.canceled += HandleMove;

            // 공격 입력이 들어올 때 HandleAttack 실행
            attackAction.performed += HandleAttack;

            // 대시 입력 (스페이스바)
            dashAction.performed += HandleDash;

            // 스킬 입력 (Interact 키)
            skillAction.performed += HandleSkill;
        }

        // OnDisable: 이 컴포넌트가 비활성화될 때 호출됨
        // 메모리 누수 방지를 위해 이벤트 구독을 반드시 해제해야 해!
        private void OnDisable()
        {
            inputActions.Disable(); // 입력 감지 중지

            // += 로 연결했으면 -= 로 반드시 해제!
            moveAction.performed -= HandleMove;
            attackAction.performed -= HandleAttack;
            dashAction.performed -= HandleDash;
            skillAction.performed -= HandleSkill;
        }

        // 이동 입력이 들어왔을 때 실제로 처리하는 함수
        // InputAction.CallbackContext ctx: 어떤 버튼이 눌렸는지 등의 정보가 담겨있음
        private void HandleMove(InputAction.CallbackContext ctx)
        {
            // Logger.Log($"Move: {ctx.ReadValue<Vector2>()}");

            // ctx.ReadValue<Vector2>(): 현재 입력된 방향값을 Vector2로 읽어옴
            // ?.Invoke(): OnMove를 구독한 곳이 있으면 이벤트를 발동시킴
            OnMove?.Invoke(ctx.ReadValue<Vector2>());
        }

        // 공격 입력이 들어왔을 때 처리하는 함수
        private void HandleAttack(InputAction.CallbackContext obj)
        {
            // Logger.Log("Attack");

            // OnAttack을 구독한 곳이 있으면 이벤트를 발동시킴
            OnAttack?.Invoke();
        }

        // 대시 입력이 들어왔을 때 처리하는 함수
        private void HandleDash(InputAction.CallbackContext obj)
        {
            OnDash?.Invoke();
        }

        // 스킬 입력이 들어왔을 때 처리하는 함수
        private void HandleSkill(InputAction.CallbackContext obj)
        {
            OnSkill?.Invoke();
        }
    }
}
