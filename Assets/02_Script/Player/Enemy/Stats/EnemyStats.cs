using UnityEngine;


namespace Litte.Enemy.Stats
{
    [CreateAssetMenu(fileName = "EnemyStats", menuName = "LittleSword/EnemyStats", order = 0)]
    public class EnemyStats : ScriptableObject
    {
        [Header("Enemy Basic Stats")]
        public int maxHP = 100;
        public float moveSpeed = 3f;

        [Header("Enemy Detection Stats")]
        public float detecInterval = 0.5f;

        [Header("Enemy Combat Stats")]
        public float chaseDistance = 5f;
        public float attackDistance = 1.5f;
        public float attackDamage = 10f;
        public float attackCooldown = 1f;
    }
}

