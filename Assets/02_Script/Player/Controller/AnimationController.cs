using UnityEngine;


namespace LittleSword.Controller
{
    // ============================================================
    // AnimationController: 플레이어 애니메이션을 제어하는 클래스
    // ============================================================
    // 유니티 Animator는 파라미터(bool, trigger 등)를 받아서 애니메이션을 전환해.
    // AnimationController는 그 파라미터를 설정하는 역할만 담당해.
    //
    // 왜 따로 분리했을까?
    //   BasePlayer에 애니메이션 코드가 섞이면 복잡해지니까,
    //   "애니메이션 담당"을 별도 클래스로 분리해서 역할을 명확히 나눈 거야.
    //   (단일 책임 원칙 - 하나의 클래스는 하나의 역할만!)
    //
    // 참고: 이 클래스는 MonoBehaviour가 아니야!
    //   → 유니티 컴포넌트로 붙이는 게 아니라 일반 C# 클래스로 new해서 씀
    // ============================================================
    public class AnimationController : MonoBehaviour
    {
        // Animator 컴포넌트 참조 (readonly = 생성 후 변경 불가)
        private readonly Animator animator;

        // ─── 애니메이터 파라미터 해시값 ─────────────────────────
        // Animator.StringToHash("파라미터이름"): 문자열을 숫자(해시)로 변환
        // 이유: 매 프레임 "IsRun" 같은 문자열로 파라미터를 찾으면 느려.
        //       미리 숫자로 변환해두면 훨씬 빠르게 접근할 수 있어! (성능 최적화)
        // static readonly: 모든 인스턴스가 공유하는 상수값
        private static readonly int hashIsRun = Animator.StringToHash("IsRun");
        private static readonly int hashAttack = Animator.StringToHash("Attack");
        private static readonly int hashDie = Animator.StringToHash("Die");
        private static readonly int hashHit = Animator.StringToHash("Hit");

        // 생성자: new AnimationController(animator)로 생성할 때 호출
        public AnimationController(Animator animator)
        {
            this.animator = animator;
        }

        // 이동 상태 애니메이션 전환
        // isMoving이 true면 달리기 애니메이션, false면 대기 애니메이션
        // SetBool: bool 파라미터를 설정 (true/false 유지됨)
        public void Move(bool isMoving)
        {
            animator.SetBool(hashIsRun, isMoving);
        }

        // 공격 애니메이션 재생
        // SetTrigger: 트리거 파라미터를 발동 (한 번만 재생되고 자동으로 꺼짐)
        public void Attack()
        {
            animator.SetTrigger(hashAttack);
        }

        // 사망 애니메이션 재생
        public void Die()
        {
            animator.SetTrigger(hashDie);
        }

        // 피격 애니메이션 재생
        public void Hit()
        {
            animator.SetTrigger(hashHit);
        }
    }
}
