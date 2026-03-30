using LittleSword.InputSystem;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using Logger = LittleSword.Common.Logger;

namespace LittleSword.InputSystem
{
    public class InputHandler : MonoBehaviour, IInputEvents
    {
        public event Action<Vector2> OnMove;
        public event Action OnAttack;

        private InputSystem_Actions inputActions;
        private InputAction moveAction;
        private InputAction attackAction;

        private void Awake()
        {
            inputActions = new InputSystem_Actions();
            moveAction = inputActions.Player.Move;
            attackAction = inputActions.Player.Attack;
        }

        private void OnEnable()
        {
            inputActions.Enable();
            moveAction.performed += HandleMove;
            moveAction.canceled += HandleMove;
            attackAction.performed += HandleAttack;
        }

        private void OnDisable()
        {
            inputActions.Disable();
            moveAction.performed -= HandleMove;
            attackAction.performed -= HandleAttack;
        }

        private void HandleMove(InputAction.CallbackContext ctx)
        {
            // Logger.Log($"Move: {ctx.ReadValue<Vector2>()}");
            OnMove?.Invoke(ctx.ReadValue<Vector2>());
        }

        private void HandleAttack(InputAction.CallbackContext obj)
        {
            // Logger.Log("Attack");

            OnAttack?.Invoke();
        }
    }
}

