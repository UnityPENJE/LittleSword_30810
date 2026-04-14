using System;
using LittleSword.Controller;
using LittleSword.InputSystem;
using UnityEngine;
using LittleSword.Interfaces;
using LittleSword.Effects;
using LittleSword.UI;
using Unity.VisualScripting;


namespace LittleSword.Player
{
    // ============================================================
    // BasePlayer: 플레이어 캐릭터의 기반(공통) 클래스
    // ============================================================
    // 게임에는 전사(Warrior), 궁수(Archer) 등 여러 직업이 있어.
    // 이들은 공통으로 이동, 공격, 피해 받기 같은 기능을 갖고 있어.
    // 그 공통 기능을 여기 BasePlayer에 모아놨어!
    //
    // Warrior와 Archer는 BasePlayer를 상속(: BasePlayer)받아서
    // 공통 기능은 그대로 물려받고, 직업별 특수 공격만 추가로 구현하면 돼.
    //
    // MonoBehaviour = 유니티 게임 오브젝트에 붙이는 스크립트 기본 클래스
    // IDamageable = 피해를 받을 수 있음을 보장하는 인터페이스
    // ============================================================
    public class BasePlayer : MonoBehaviour, IDamageable
    {
        // ─── 컨트롤러 ───────────────────────────────────────────
        // 입력을 받는 컴포넌트 (키보드/패드 입력 감지)
        private InputHandler inputHandler;

        // 이동 처리 전용 컨트롤러 (속도, 방향 전환 처리)
        // protected = 이 클래스와 자식 클래스(Warrior, Archer)에서만 접근 가능
        protected MovementController movementController;

        // 애니메이션 전환 전용 컨트롤러 (달리기, 공격, 피격 등)
        private AnimationController animationController;

        // ─── 유니티 컴포넌트 ─────────────────────────────────────
        // 물리 처리 (이동, 충돌 등)
        protected Rigidbody2D rigidBody;

        // 스프라이트 이미지 렌더링 (좌우 반전 등)
        protected SpriteRenderer spriteRenderer;

        // 애니메이션 재생
        protected Animator animator;

        // 충돌 감지 영역
        protected Collider2D collider;

        // 피격 시 스프라이트 깜빡임 효과
        private HitFlash hitFlash;

        // 대시(회피) 컨트롤러
        private DashController dashController;

        // ─── 스탯 & 상태 ─────────────────────────────────────────
        // ScriptableObject로 만든 플레이어 능력치 데이터
        public PlayerStats playerStats;

        // 다음 공격이 가능한 시간 (쿨다운 계산용)
        private float nextAttackTime = 0f;

        // IDamageable 인터페이스 구현:
        // => 표현식: CurrentHP가 0 이하면 IsDead = true
        public bool IsDead => CurrentHP <= 0;

        // 현재 체력 (set은 public으로 BasePlayerEditor에서 수정 가능)
        public int CurrentHP { get; set; }

        // HP가 변경되었을 때 발생하는 이벤트 (현재HP, 최대HP)
        // HP Bar UI 등에서 구독해서 체력 변화를 감지
        public event Action<int, int> OnHPChanged;

        // ─── 유니티 생명주기 함수 ────────────────────────────────

        // Awake: 오브젝트가 생성될 때 가장 먼저 호출 (Start보다 먼저)
        protected void Awake()
        {
            InitComponents(); // 유니티 컴포넌트 초기화
            InitControllers(); // 입력/이동/애니메이션 컨트롤러 초기화
        }

        // OnEnable: 오브젝트가 활성화될 때 호출
        // 입력 이벤트를 구독해서 키 입력을 받기 시작
        protected void OnEnable()
        {
            inputHandler.OnMove += Move;     // 이동 입력 → Move 함수 연결
            inputHandler.OnAttack += Attack; // 공격 입력 → Attack 함수 연결
            inputHandler.OnDash += OnDash;   // 대시 입력 → 대시 실행 연결
        }

        // OnDisable: 오브젝트가 비활성화될 때 호출
        // 이벤트 구독을 해제해서 메모리 누수 방지
        protected void OnDisable()
        {
            inputHandler.OnMove -= Move;
            inputHandler.OnAttack -= Attack;
            inputHandler.OnDash -= OnDash;
        }

        // 컨트롤러 클래스들을 초기화 (유니티 컴포넌트가 먼저 있어야 해서 InitComponents 다음에 호출)
        private void InitControllers()
        {
            // 같은 게임 오브젝트에서 InputHandler 컴포넌트를 찾아 가져옴
            inputHandler = GetComponent<InputHandler>();

            // MovementController는 MonoBehaviour가 아닌 일반 클래스라서 new로 생성
            movementController = new MovementController(rigidBody, spriteRenderer);
            animationController = new AnimationController(animator);
        }

        // 유니티 컴포넌트들을 찾아서 변수에 저장
        private void InitComponents()
        {
            rigidBody = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            collider = GetComponent<Collider2D>();

            hitFlash = GetComponent<HitFlash>();
            dashController = GetComponent<DashController>();

            // 2D 탑다운 게임이라 중력 없이 이동하도록 중력 끔
            rigidBody.gravityScale = 0;

            // 물리 충돌로 인해 캐릭터가 회전하지 않도록 고정
            rigidBody.freezeRotation = true;

            // 게임 시작 시 HP를 최대 체력으로 설정
            CurrentHP = playerStats.maxHP;

            // HP 초기값을 이벤트로 알려줌 (HP Bar 초기화용)
            OnHPChanged?.Invoke(CurrentHP, playerStats.maxHP);
        }

        // ─── 주요 기능 함수 ──────────────────────────────────────

        // 이동 처리 - 입력된 방향으로 캐릭터를 움직임
        // virtual: 자식 클래스(Warrior, Archer)에서 이 함수를 재정의(override) 할 수 있어
        protected virtual void Move(Vector2 direction)
        {
            // Rigidbody에 직접 속도를 설정해서 이동
            rigidBody.linearVelocity = direction * 3.0f;

            // 대시 컨트롤러에 현재 이동 방향 전달 (대시 방향으로 사용)
            dashController?.SetMoveDirection(direction);

            // MovementController에도 이동 처리 위임 (스프라이트 방향 전환 포함)
            movementController.Move(direction, playerStats.moveSpeed);

            // 이동 중이면 달리기 애니메이션, 멈추면 대기 애니메이션으로 전환
            animationController.Move(direction != Vector2.zero);
        }

        // 공격 처리 - 쿨다운이 끝났을 때만 공격 가능
        protected virtual void Attack()
        {
            // Time.time: 게임 시작 후 경과된 시간(초)
            // nextAttackTime 이상이 되면 공격 가능
            if (Time.time >= nextAttackTime)
            {
                animationController.Attack(); // 공격 애니메이션 재생

                // 다음 공격 가능 시간 = 현재 시간 + 쿨다운
                nextAttackTime = Time.time + playerStats.Attackcooldwon;
            }
        }

        // IDamageable 인터페이스 구현: 피해를 받는 함수
        public void TakeDamage(int damage)
        {
            // 이미 죽어있으면 피해 무시
            if (IsDead) return;

            // 대시 중 무적 상태면 피해 무시
            if (dashController != null && dashController.IsInvincible) return;

            // HP를 깎되, 최소값은 0 (음수 방지)
            CurrentHP = Mathf.Max(0, CurrentHP - damage);

            // HP 변경 이벤트 발생 (HP Bar 갱신용)
            OnHPChanged?.Invoke(CurrentHP, playerStats.maxHP);

            // 데미지 팝업 표시
            DamagePopupSpawner.Instance?.Spawn(transform.position, damage);

            if (IsDead)
            {
                Die(); // HP가 0이 되면 사망 처리
            }
            else
            {
                animationController.Hit(); // 살아있으면 피격 애니메이션 재생

                // 피격 이펙트: 카메라 흔들림 + 스프라이트 깜빡임
                CameraShake.Instance?.Shake();
                hitFlash?.Flash();
            }
        }

        // 대시 입력 처리
        private void OnDash()
        {
            dashController?.TriggerDash();
        }

        // 체력 회복 함수 (아이템 등에서 호출)
        // amount만큼 HP를 회복하되, 최대 HP를 초과하지 않음
        public void Heal(int amount)
        {
            if (IsDead) return;

            CurrentHP = Mathf.Min(CurrentHP + amount, playerStats.maxHP);
            OnHPChanged?.Invoke(CurrentHP, playerStats.maxHP);
        }

        // 사망 처리
        void Die()
        {
            animationController.Die(); // 사망 애니메이션 재생

            inputHandler.enabled = false;          // 더 이상 입력 받지 않음
            collider.enabled = false;              // 충돌 비활성화 (적이 뚫고 지나갈 수 있게)
            rigidBody.linearVelocity = Vector2.zero; // 이동 멈춤
        }
    }
}
