using UnityEngine;


namespace LittleSword.Controller
{
    

    public class MovementController : MonoBehaviour
    {

        private readonly Rigidbody2D rigidBody;
        private readonly SpriteRenderer spriteRenderer;
        
        public MovementController(Rigidbody2D rigidBody, SpriteRenderer spriteRenderer)
        {
            this.rigidBody = rigidBody;
            this.spriteRenderer = spriteRenderer;
        }

        public void Move(Vector2 direction, float moveSpeed)
        {
            rigidBody.linearVelocity = direction * moveSpeed;

            if(direction != Vector2.zero)
            {
                spriteRenderer.flipX = direction.x < 0;
            }
        }
    }
}

