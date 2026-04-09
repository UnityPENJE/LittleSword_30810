namespace LittleSword.Enemy.FSM
{
    // ============================================================
    // StateMachine: FSM(유한 상태 기계)을 관리하는 클래스
    // ============================================================
    // StateMachine은 현재 상태가 무엇인지 기억하고,
    // 상태 전환(ChangeState)과 매 프레임 업데이트(Update)를 담당해.
    //
    // 동작 방식:
    //   1. Enemy.Start()에서 StateMachine 생성 후 Idle 상태로 시작
    //   2. Enemy.Update()에서 매 프레임 stateMachine.Update() 호출
    //   3. 각 상태의 Update()에서 조건 판단 후 ChangeState() 호출
    //   4. ChangeState(): 현재 상태 Exit → 새 상태로 교체 → 새 상태 Enter
    // ============================================================
    public class StateMachine
    {
        // 이 상태 기계를 소유한 적 오브젝트 (각 상태 함수에 전달됨)
        private Enemy enemy;

        // 생성자: 어떤 적의 상태 기계인지 받아옴
        public StateMachine(Enemy enemy)
        {
            this.enemy = enemy;
        }

        // 현재 활성화된 상태 (외부에서는 읽기만 가능, 변경은 ChangeState로만)
        public IState currentState { get; private set; }

        // 상태 전환 함수
        public void ChangeState(IState newState)
        {
            // ?. (null 조건 연산자): currentState가 null이면 Exit 호출 안 함
            // 처음 상태를 설정할 때는 currentState가 null이라서 Exit가 필요 없어
            currentState?.Exit(enemy); // 현재 상태 종료 처리

            currentState = newState;   // 새 상태로 교체

            currentState.Enter(enemy); // 새 상태 진입 처리
        }

        // 매 프레임 호출 - 현재 상태의 Update를 실행
        public void Update()
        {
            // currentState가 null이면 실행 안 함 (게임 시작 직후 안전 처리)
            currentState?.Update(enemy);
        }
    }
}
