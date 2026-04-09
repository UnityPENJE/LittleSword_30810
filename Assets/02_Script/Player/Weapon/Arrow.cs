using LittleSword.Interfaces;
using UnityEngine;

// ============================================================
// Arrow: 궁수가 발사하는 화살 클래스
// ============================================================
// Archer가 FireArrow()를 호출하면 화살 프리팹이 씬에 생성되고,
// 이 Arrow 스크립트가 실행돼.
//
// 화살 동작 흐름:
//   1. Archer가 Instantiate()로 화살 오브젝트 생성
//   2. Init()으로 힘과 데미지 설정
//   3. Start()에서 Rigidbody에 힘을 가해 날아감
//   4. 적에 맞으면 OnTriggerEnter2D()에서 데미지 주고 화살 제거
//   5. 3초 후 아무것도 안 맞으면 자동으로 제거 (Destroy)
// ============================================================
public class Arrow : MonoBehaviour
{
    // Rigidbody2D: 화살에 물리적인 힘을 가하기 위한 컴포넌트
    private Rigidbody2D rigidbody;

    // 화살 날아가는 힘 (기본값 10, Archer의 playerStats.fireForce로 덮어씌워짐)
    public float force = 10.0f;

    // 화살이 주는 피해량 (Archer의 playerStats.attackDamage로 덮어씌워짐)
    public int damage = 30;

    // 화살 생성 직후 Archer에서 호출되는 초기화 함수
    // Init이 먼저 호출되고, 그 다음 Start가 호출됨
    public void Init(float force, int damage)
    {
        this.force = force;   // this.force: 클래스 멤버 변수 / force: 파라미터 구분
        this.damage = damage;
    }

    // Start: 오브젝트가 씬에 생성된 후 첫 프레임에 호출
    private void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();

        // AddRelativeForce: 오브젝트 자신의 기준 방향으로 힘을 가함
        // transform.right: 오브젝트가 바라보는 오른쪽 방향 (회전이 적용된 방향)
        // ForceMode2D.Impulse: 한 번에 순간적인 힘을 가함 (총알처럼 즉시 속도가 생김)
        rigidbody.AddRelativeForce(transform.right * force, ForceMode2D.Impulse);

        // 3초 후 화살 게임 오브젝트를 자동으로 삭제 (벽에 안 맞아도 없어지게)
        Destroy(gameObject, 3.0f);
    }

    // OnTriggerEnter2D: 트리거 콜라이더가 다른 콜라이더와 겹쳤을 때 호출
    // (일반 충돌과 달리, 트리거는 통과는 하지만 겹침을 감지할 수 있어)
    private void OnTriggerEnter2D(Collider2D other)
    {
        // "Enemy" 태그를 가진 오브젝트에 맞았을 때만 처리
        if (other.CompareTag("Enemy"))
        {
            // IDamageable 인터페이스가 있으면 데미지를 줌
            // ?.TakeDamage(): null이면 실행 안 함 (안전 호출)
            other.GetComponent<IDamageable>()?.TakeDamage(damage);

            // 적에 맞으면 화살 즉시 제거
            Destroy(gameObject);
        }
    }
}
