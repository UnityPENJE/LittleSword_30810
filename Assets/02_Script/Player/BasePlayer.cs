using LittleSword.Controller;
using LittleSword.InputSystem;
using UnityEngine;
using LittleSword.Interfaces;


namespace LittleSword.Player
{
    public class BasePlayer : MonoBehaviour, IDamageable
    {
        // Controllers
        private InputHandler inputHandler;

        // 이동 제어용 컨트롤러 참조(하위 클래스에서 사용 가능)
        protected MovementController movementController;

        private AnimationController animationController;

        // components
        protected Rigidbody2D rigidBody;
        protected SpriteRenderer spriteRenderer;
        protected Animator animator;
        protected Collider2D collider;

        public PlayerStats playerStats;

        public bool IsDead => CurrentHP <= 0;
        public int CurrentHP { get; set; }

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
        }

        protected virtual void Move(Vector2 direction)
        {
            rigidBody.linearVelocity = direction * 3.0f;
            movementController.Move(direction, playerStats.moveSpeed);
            animationController.Move(direction != Vector2.zero);
        }

        protected virtual void Attack()
        {
            animationController.Attack();
        }

        public void TakeDamage(int damage)
        {
            if (IsDead) return;

            CurrentHP = Mathf.Max(0, CurrentHP - damage);

            if (IsDead)
            {
                Die();
            }
            else
            {
                animationController.Hit();
            }
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

