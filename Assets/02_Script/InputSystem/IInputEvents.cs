using System;
using UnityEngine;

namespace LittleSword.InputSystem
{
    public interface IInputEvents
    {
        event Action<Vector2> OnMove;

        event Action OnAttack;
    }
}
