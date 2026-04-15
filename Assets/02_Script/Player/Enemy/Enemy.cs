using Litte.Enemy.Stats;
using LittleSword.Effects;
using LittleSword.Enemy.FSM;
using LittleSword.Interfaces;
using LittleSword.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;


namespace LittleSword.Enemy
{
    // ============================================================
    // Enemy: 적 캐릭터의 핵심 클래스
    // ============================================================
    // 적의 모든 동작을 FSM(유한 상태 기계)으로 관리해.
    // 상태: Idle(대기) → Chase(추격) → Attack(공격) → Die(사망)
    //
    // 이 클래스의 역할:
    //   - 적의 컴포넌트(Rigidbody, Animator 등) 초기화
    //   - 상태 객체들을 Dictionary로 관리
    //   - 플레이어 감지, 이동, 공격 판정 등 실제 행동 함수 제공
    //   → 각 상태(IState)에서 이 함수들을 호출해서 행동을 수행해
    //
    // IDamageable = 피해를 받을 수 있음을 보장하는 인터페이스
    // ============================================================
    public class Enemy : MonoBehaviour, IDamageable
    {
        // ─── FSM 관련 ────────────────────────────────────────────
        // 상태 기계 인스턴스
        private StateMachine stateMachine;
        public StateMachine StateMachine => stateMachine; // 외부 읽기 전용 접근

        // 현재 상태 이름 (EnemyEditor에서 표시용)
        // GetType().Name: 클래스 이름을 문자열로 반환 (예: "ChaseState")
        // ?? "None": stateMachine이나 currentState가 null이면 "None" 반환
        public string CurrentStateName => stateMachine?.currentState?.GetType().Name ?? "None";

        // 모든 상태를 타입으로 관리하는 딕셔너리
        // Dictionary<키, 값> = 키-값 쌍의 컬렉션 (자료구조)
        // typeof(IdleState)를 키로, 실제 상태 객체를 값으로 저장
        private Dictionary<Type, IState> states;

        // ─── 유니티 컴포넌트 ─────────────────────────────────────
        // [NonSerialized]: 인스펙터에서 안 보이게 하지만 public 유지 (다른 클래스에서 접근 가능)
        [NonSerialized] public Rigidbody2D rigidbody;
        [NonSerialized] public SpriteRenderer spriteRenderer;
        [NonSerialized] public Animator animator;

        // ─── 애니메이터 파라미터 해시 ────────────────────────────
        // static = 모든 Enemy 인스턴스가 공유 (메모리 절약)
        // readonly = 값 변경 불가
        public static readonly int hashIsRun = Animator.StringToHash("IsRun");
        public static readonly int hashAttack = Animator.StringToHash("Attack");
        public static readonly int hashDie = Animator.StringToHash("Die");
        public static readonly int hashHit = Animator.StringToHash("Hit");

        // ─── 스탯 & 상태 ─────────────────────────────────────────
        [SerializeField] private EnemyStats enemyStats; // 적 능력치 ScriptableObject

        // 추격/공격 대상 플레이어 Transform
        [SerializeField] private Transform target;
        public Transform Target => target; // 외부 읽기 전용

        // IDamageable 구현
        public bool IsDead => CurrentHP <= 0;
        public int CurrentHP { get; private set; } // Enemy 내부에서만 변경 가능
        public Action<int, int> OnHPChanged { get; internal set; }

        // 플레이어가 속한 레이어 (OverlapCircle로 감지할 때 이 레이어만 탐색)
        public LayerMask playerLayer;

        // ─── 유니티 생명주기 ─────────────────────────────────────

        private void Awake()
        {
            InisState();       // 상태 딕셔너리 초기화
            InitComponents();  // 유니티 컴포넌트 초기화
        }

        private void Start()
        {
            // StateMachine은 Enemy 초기화 후 Start에서 생성
            stateMachine = new StateMachine(this);

            // 게임 시작 시 Idle(대기) 상태로 시작
            ChangeState<Idlestate>();
        }

        private void Update()
        {
            // 매 프레임 현재 상태의 Update() 실행
            stateMachine.Update();
        }

        // 상태 딕셔너리 초기화
        // 각 상태 타입 → 상태 객체를 미리 생성해둬 (상태 전환 시 new 없이 재사용)
        private void InisState()
        {
            states = new Dictionary<Type, IState>
            {
                [typeof(Idlestate)]   = new Idlestate(enemyStats.detecInterval),
                [typeof(ChaseState)]  = new ChaseState(enemyStats.detecInterval),
                [typeof(AttackState)] = new AttackState(enemyStats.attackCooldown),
                [typeof(DieState)]    = new DieState()
            };
        }

        // 유니티 컴포넌트 가져오기 & 초기 설정
        private void InitComponents()
        {
            rigidbody = GetComponent<Rigidbody2D>();

            // 2D 탑다운 게임이라 중력 없이 이동
            rigidbody.gravityScale = 0;

            // 물리 충돌로 인한 회전 방지
            rigidbody.freezeRotation = true;

            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();

            // 시작 시 HP를 최대값으로 설정
            CurrentHP = enemyStats.maxHP;
        }

        // ─── 상태 전환 ────────────────────────────────────────────

        // 제네릭 상태 전환 함수
        // where T : IState → T는 반드시 IState를 구현한 타입이어야 해
        // 예: enemy.ChangeState<ChaseState>();
        public void ChangeState<T>() where T : IState
        {
            // 죽은 상태에서는 DieState로만 전환 가능 (다른 상태로 못 감)
            if (IsDead && typeof(T) != typeof(DieState)) return;

            // 딕셔너리에서 T 타입에 해당하는 상태를 찾아서 전환
            if (states.TryGetValue(typeof(T), out IState newstate))
            {
                stateMachine.ChangeState(newstate);
            }
        }

        // ─── 행동 함수들 (각 상태에서 호출) ─────────────────────

        // 주변에 플레이어가 있는지 원형 범위로 감지
        // 반환값: 살아있는 플레이어가 감지되면 true, 아니면 false
        public bool DetectPlayer()
        {
            // OverlapCircleAll: 원형 범위 안의 모든 콜라이더를 배열로 반환
            // 파라미터: (중심 위치, 반지름, 감지할 레이어)
            Collider2D[] colliders =
                Physics2D.OverlapCircleAll(transform.position, enemyStats.chaseDistance, playerLayer);

            if (colliders.Length > 0)
            {
                // 가장 가까운 살아있는 플레이어를 target으로 설정
                // OrderBy: 거리 기준으로 정렬 (sqrMagnitude = 제곱 거리, sqrt 계산 없어서 빠름)
                // Where: 죽지 않은 플레이어만 필터링
                // First(): 가장 첫 번째(가장 가까운) 콜라이더 선택
                target = colliders
                    .OrderBy(c => (transform.position - c.transform.position).sqrMagnitude)
                    .Where(c => c.GetComponent<BasePlayer>()?.IsDead == false)
                    .First()
                    .transform;

                return target != null;
            }

            target = null;
            return false;
        }

        // 플레이어 방향으로 이동
        public void MoveToPlayer()
        {
            if (target == null) return;

            SetFacing(); // 플레이어 방향으로 스프라이트 반전

            // 목표 방향 정규화 (.normalized = 방향만 남기고 크기를 1로 만듦)
            Vector2 direction = (target.position - transform.position).normalized;

            rigidbody.linearVelocity = direction * enemyStats.moveSpeed;
        }

        // 플레이어 방향으로 스프라이트 반전 (바라보는 방향 설정)
        public void SetFacing()
        {
            if (spriteRenderer == null) return;
            Vector2 dir = target.position - transform.position;
            spriteRenderer.flipX = dir.x < 0; // 플레이어가 왼쪽이면 반전
        }

        // 이동 멈춤
        public void StopMoving()
        {
            rigidbody.linearVelocity = Vector2.zero;
        }

        // 플레이어가 공격 범위 안에 있는지 확인
        public bool IsInAttackRange()
        {
            if (target == null) return false;

            // sqrMagnitude로 거리 비교 (제곱 거리끼리 비교 = sqrt 연산 생략으로 성능 향상)
            float targetDisTance = (transform.position - target.position).sqrMagnitude;

            // attackDistance²와 비교 (제곱 거리끼리 비교)
            return targetDisTance <= enemyStats.attackDistance * enemyStats.attackDistance;
        }

        // 키보드로 FSM을 테스트하는 디버그 함수 (Update에서 주석 처리됨)
        private void TestFSM()
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) ChangeState<Idlestate>();
            if (Keyboard.current.digit2Key.wasPressedThisFrame) ChangeState<ChaseState>();
            if (Keyboard.current.digit3Key.wasPressedThisFrame) ChangeState<AttackState>();
        }

        // 유니티 에디터 씬 뷰에서 탐지/공격 범위를 원으로 시각화
        private void OnDrawGizmos()
        {
            // 파란 원 = 플레이어 추격을 시작하는 감지 범위
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, enemyStats.chaseDistance);

            // 빨간 원 = 공격이 가능한 범위
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, enemyStats.attackDistance);
        }

        // 애니메이션 이벤트에서 호출 - 공격 판정이 들어가는 순간에 실행
        public void OnAttackAnimationEvent()
        {
            if (target == null) return;

            // 타겟에 IDamageable이 있으면 데미지 적용
            target.GetComponent<IDamageable>()?.TakeDamage(enemyStats.attackDamage);
            CameraShake.Instance?.Shake();
        }

        // IDamageable 인터페이스 구현: 피해 받기
        public void TakeDamage(int damage)
        {
            if (IsDead) return; // 이미 죽었으면 무시

            CurrentHP -= damage;

            if (IsDead)
            {
                Die(); // HP가 0 이하면 사망
            }
            else
            {
                animator.SetTrigger(hashHit); // 살아있으면 피격 애니메이션
            }
        }

        // 사망 처리: DieState로 전환
        public void Die()
        {
            ChangeState<DieState>(); // DieState에서 애니메이션, 충돌 비활성화, 오브젝트 제거 처리
        }
    }
}
