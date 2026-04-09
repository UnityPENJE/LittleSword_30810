using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace LittleSword.Common
{
    // ============================================================
    // Logger: 게임 개발용 로그 출력 유틸리티 클래스
    // ============================================================
    // 일반 Debug.Log를 직접 쓰면 게임을 출시(빌드)할 때도 로그가 남아서
    // 성능이 느려질 수 있어. 이 Logger는 에디터나 개발 모드에서만
    // 로그를 출력하고, 실제 출시 빌드에서는 자동으로 꺼지도록 만들어졌어.
    // ============================================================
    public static class Logger
    {
        // [Conditional("...")] : 특정 심볼이 정의된 환경에서만 이 함수가 실행돼.
        // DEVELOP_MODE 또는 UNITY_EDITOR 심볼이 있을 때만 로그가 출력됨.
        // → 즉, 유니티 에디터 안에서 테스트할 때만 콘솔에 찍힌다는 뜻!

        // 일반 정보 로그 출력 (흰색)
        [Conditional("DEVELOP_MODE")]
        [Conditional("UNITY_EDITOR")]
        public static void Log(object message)
        {
            Debug.Log(message);
        }

        // 에러 로그 출력 (빨간색) - 뭔가 잘못됐을 때 사용
        [Conditional("DEVELOP_MODE")]
        [Conditional("UNITY_EDITOR")]
        public static void logError(object message)
        {
            Debug.LogError(message);
        }

        // 경고 로그 출력 (노란색) - 심각하진 않지만 주의가 필요할 때 사용
        [Conditional("DEVELOP_MODE")]
        [Conditional("UNITY_EDITOR")]
        public static void logWarning(object message)
        {
            Debug.LogWarning(message);
        }
    }
}
