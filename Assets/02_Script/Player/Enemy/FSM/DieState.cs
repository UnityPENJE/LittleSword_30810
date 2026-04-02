using UnityEngine;

namespace LittleSword.Enemy.FSM
{
    public class DieState : IState
    {
        public void Enter(Enemy enemy)
        {
            enemy.animator.SetTrigger(Enemy.hashDie); // 사망 트리거 작동
            enemy.StopMoving(); // 이동 멈춤
            enemy.GetComponent<Collider2D>().enabled = false; // 충돌 비활성화
            enemy.rigidbody.bodyType = RigidbodyType2D.Kinematic; // 물리연산 중단
            Object.Destroy(enemy.gameObject, 5.0f); // 일정시간 후 -> 오브젝트 제거


        }

        public void Update(Enemy enemy) { }
        public void Exit(Enemy enemy) { }
    }
}

