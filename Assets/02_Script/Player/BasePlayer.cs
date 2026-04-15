using LittleSword.Controller;
using LittleSword.Effects;
using LittleSword.InputSystem;
using LittleSword.Interfaces;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;   // ← 추가


namespace LittleSword.Player
{
    public class BasePlayer : MonoBehaviour, IDamageable
    {
        // ─── 컨트롤러 ───────────────────────────────────────────
        private InputHandler inputHandler;
        protected MovementController movementController;
        private AnimationController animationController;

        // ─── 유니티 컴포넌트 ─────────────────────────────────────
        protected Rigidbody2D rigidBody;
        protected SpriteRenderer spriteRenderer;
        protected Animator animator;
        protected Collider2D collider;

        // ─── 스탯 & 상태 ─────────────────────────────────────────
        public PlayerStats playerStats;
        private float nextAttackTime = 0f;

        public bool IsDead => CurrentHP <= 0;
        public int CurrentHP { get; set; }
        public Action<int, int> OnHPChanged { get; internal set; }

        // ─── HP 바 UI (인스펙터에서 Fill Image 드래그) ───────────
        [Header("HP 바 UI")]
        [SerializeField] private Image hpFillImage;       // HPBar_Fill 드래그
        [SerializeField] private float hpLerpSpeed = 5f;  // 부드럽게 감소하는 속도
        private float targetHpFill = 1f;

        // ─── 유니티 생명주기 ─────────────────────────────────────

        protected void Awake()
        {
            InitComponents();
            InitControllers();
        }

        protected void OnEnable()
        {
            inputHandler.OnMove += Move;
            inputHandler.OnAttack += Attack;
        }

        protected void OnDisable()
        {
            inputHandler.OnMove -= Move;
            inputHandler.OnAttack -= Attack;
        }

        // HP 바를 매 프레임 부드럽게 갱신
        private void Update()
        {
            if (hpFillImage != null)
                hpFillImage.fillAmount = Mathf.Lerp(hpFillImage.fillAmount, targetHpFill, Time.deltaTime * hpLerpSpeed);
        }

        private void InitControllers()
        {
            inputHandler = GetComponent<InputHandler>();
            movementController = new MovementController(rigidBody, spriteRenderer);
            animationController = new AnimationController(animator);
        }

        private void InitComponents()
        {
            rigidBody = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            collider = GetComponent<Collider2D>();

            rigidBody.gravityScale = 0;
            rigidBody.freezeRotation = true;

            // HP 초기화
            CurrentHP = playerStats.maxHP;
            targetHpFill = 1f;
            if (hpFillImage != null) hpFillImage.fillAmount = 1f;
        }

        // ─── 주요 기능 함수 ──────────────────────────────────────

        protected virtual void Move(Vector2 direction)
        {
            rigidBody.linearVelocity = direction * 3.0f;
            movementController.Move(direction, playerStats.moveSpeed);
            animationController.Move(direction != Vector2.zero);
        }

        protected virtual void Attack()
        {
            if (Time.time >= nextAttackTime)
            {
                animationController.Attack();
                nextAttackTime = Time.time + playerStats.Attackcooldwon;
            }
        }

        public void TakeDamage(int damage)
        {
            if (IsDead) return;

            CurrentHP = Mathf.Max(0, CurrentHP - damage);

            // HP 바 목표값 갱신 (Update에서 부드럽게 줄어듦)
            targetHpFill = (float)CurrentHP / playerStats.maxHP;

            if (IsDead)
            {
                Die();
            }
            else
            {
                CameraShake.Instance?.Shake();
                animationController.Hit();
            }
        }

        // 체력 회복 (필요할 때 사용)
        public void Heal(int amount)
        {
            if (IsDead) return;
            CurrentHP = Mathf.Min(CurrentHP + amount, playerStats.maxHP);
            targetHpFill = (float)CurrentHP / playerStats.maxHP;
        }

        void Die()
        {
            animationController.Die();
            inputHandler.enabled = false;
            collider.enabled = false;
            rigidBody.linearVelocity = Vector2.zero;
        }
    }
}