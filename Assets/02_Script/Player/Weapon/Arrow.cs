using LittleSword.Interfaces;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    private Rigidbody2D rigidbody;

    public float force = 10.0f;
    public int damage = 30;

    public void Init(float force, int damage)
    {
        this.force = force;
        this.damage = damage;
    }

    private void Start()
    {

        rigidbody = GetComponent<Rigidbody2D>();

        //transform.rigiht 방향으로 상대 힘을 가함
        rigidbody.AddRelativeForce(transform.right * force, ForceMode2D.Impulse);

        // 일정 시간후 자동 제거
        Destroy(gameObject, 3.0f);

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // IDamageable을 구현한 컴포넌트에 데미지 전달
            other.GetComponent<IDamageable>()?.TakeDamage(damage);

            //적중시 화살 제거
            Destroy(gameObject);
        }
    }
}
