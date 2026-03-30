using LittleSword.Controller;
using LittleSword.InputSystem;
using UnityEngine;
using Logger = LittleSword.Common.Logger;


namespace LittleSword.Player
{
    public class BasePlayer : MonoBehaviour
    {
        // Controllers
        private InputHandler inputHandler;

        // 이동 제어용 컨트롤러 참조(하위 클래스에서 사용 가능)
        protected MovementController movementController;

        // components
        protected Rigidbody2D rigidBody;
        protected SpriteRenderer spriteRenderer;

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
        }

        private void InitComponents()
        {
            rigidBody = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        protected virtual void Move(Vector2 direction)
        {
            Logger.Log($"이동 + {direction}");

            rigidBody.linearVelocity = direction * 3.0f;

            movementController.Move(direction, moveSpeed: 5.0f);
        }

        protected virtual void Attack()
        {
            Logger.Log("공격");
        }
    }
}

