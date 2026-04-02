using UnityEngine;
using LittleSword.Enemy.FSM;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;
using Litte.Enemy.Stats;
using System.Linq;
using LittleSword.Interfaces;
using LittleSword.Player;


namespace LittleSword.Enemy
{
    public class Enemy : MonoBehaviour, IDamageable
    {
        private StateMachine stateMachine;
        public StateMachine StateMachine => stateMachine;
        public string CurrentStateName => stateMachine?.currentState?.GetType().Name ?? "None";

        private Dictionary<Type, IState> states;

        [NonSerialized] public Rigidbody2D rigidbody;
        [NonSerialized] public SpriteRenderer spriteRenderer;
        [NonSerialized] public Animator animator;

        public static readonly int hashIsRun = Animator.StringToHash("IsRun");
        public static readonly int hashAttack = Animator.StringToHash("Attack");
        public static readonly int hashDie= Animator.StringToHash("Die");
        public static readonly int hashHit = Animator.StringToHash("Hit");

        [SerializeField] private EnemyStats enemyStats;

        [SerializeField] private Transform target;
        
        
        public Transform Target => target;
        
        public bool IsDead => CurrentHP <= 0;

        public int CurrentHP { get; private set; }

        public LayerMask playerLayer;
        
     
        private void Awake()
        {
            InisState();
            InitComponents();
        }

        private void Start()
        {
            stateMachine = new StateMachine(this);
            
            ChangeState<Idlestate>();
        }

        private void Update()
        {
            stateMachine.Update();

            //TestFSM();
        }

        private void InisState()
        {
            states = new Dictionary<Type, IState>
            {
                [typeof(Idlestate)] = new Idlestate(enemyStats.detecInterval),
                [typeof(ChaseState)] = new ChaseState(enemyStats.detecInterval),
                [typeof(AttackState)] = new AttackState(enemyStats.attackCooldown),
                [typeof(DieState)] = new DieState()
            };
        }

        private void InitComponents()
        {
            rigidbody = GetComponent<Rigidbody2D>();

            rigidbody.gravityScale = 0;
            rigidbody.freezeRotation = true;
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            CurrentHP = enemyStats.maxHP;
        }

        //상태 전환 메서드
        public void ChangeState<T>() where T : IState
        {
            if (IsDead && typeof(T) != typeof(DieState)) return;
            if(states.TryGetValue(typeof(T), out IState newstate))
            {
                stateMachine.ChangeState(newstate);
            }
        }

        public bool DetectPlayer()
        {
            Collider2D[] colliders =
                Physics2D.OverlapCircleAll(transform.position, enemyStats.chaseDistance, playerLayer);

            if (colliders.Length > 0)
            {
                target = colliders
                    .OrderBy(c => (transform.position - c.transform.position).sqrMagnitude)
                    .Where(c=>c.GetComponent<BasePlayer>()?.IsDead == false)
                    .First()
                    .transform;

                return target != null;
            }

            target = null;
            return false;
        }

        public void MoveToPlayer()
        {
            if(target == null) return;

            SetFacing();

            Vector2 direction = (target.position - transform.position).normalized;
            
            rigidbody.linearVelocity = direction * enemyStats.moveSpeed;
        }

        public void SetFacing()
        {
            if(spriteRenderer == null) return;
            Vector2 dir = target.position - transform.position;
            spriteRenderer.flipX = dir.x < 0;
        }

        public void StopMoving()
        {
            rigidbody.linearVelocity = Vector2.zero;
        }

        public bool IsInAttackRange()
        {
            if(target == null) return false;

            float targetDisTance = (transform.position - target.position).sqrMagnitude;

            return targetDisTance <= enemyStats.attackDistance * enemyStats.attackDistance;
        }
        private void TestFSM()
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                ChangeState<Idlestate>();
            }
            if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                ChangeState<ChaseState>();
            }
            if (Keyboard.current.digit3Key.wasPressedThisFrame)
            {
                ChangeState<AttackState>();
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, enemyStats.chaseDistance);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, enemyStats.attackDistance);
        }

        public void OnAttackAnimationEvent()
        {
            if(target == null) return;

            target.GetComponent<IDamageable>()?.TakeDamage(enemyStats.attackDamage);
        }

        public void TakeDamage(int damage)
        {
            if(IsDead) return;
            CurrentHP -= damage;
            if (IsDead)
            {
                Die();
            }
            else
            {
                animator.SetTrigger(hashHit);
            }
        }

        public void Die()
        {
            // animator.SetTrigger(hashDie);
            ChangeState<DieState>(); // 죽은 상태 전환
        }
    }
}

