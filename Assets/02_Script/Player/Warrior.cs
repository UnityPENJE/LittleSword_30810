using LittleSword.Interfaces;
using UnityEngine;

namespace LittleSword.Player
{
    public class Warrior : BasePlayer
    {
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private Vector2 size = new Vector2(1.0f, 2.0f);

        [SerializeField] private float offset = 0.5f;

        // 공격 판정 메서드
        public void OnWarriorAttack()
        {
            // 플레이어 반향에 따른 공격 판정 변경 계산
            Vector2 direction = spriteRenderer.flipX ? Vector2.left : Vector2.right;

            // 공격 판정 로직
            Vector2 center = (Vector2)transform.position + direction * offset;

            // 범위 안에 있는 모든 collider를 찾아서 리스트에 저장
            Collider2D[] colliders = Physics2D.OverlapBoxAll(center,size, 0, enemyLayer);

            // 찾은 collider를 돌아가며 작동
            foreach (var collider in colliders)
            {
                collider.GetComponent<IDamageable>()?.TakeDamage(playerStats.attackDamage);
            }
        }

        private void OnDrawGizmos()
        {
            if(spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

            Vector2 direction = spriteRenderer.flipX ? Vector2.left : Vector2.right;

            // 공격 판정 로직
            Vector2 center = (Vector2)transform.position + direction * offset;


            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawCube(center, new Vector3(size.x,size.y, 0.0f));
        }
    }
}

