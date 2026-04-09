using UnityEngine;
using LittleSword.Player;
using UnityEditor;

// ============================================================
// BasePlayerEditor: 유니티 에디터에서 플레이어 정보를 보여주는 커스텀 인스펙터
// ============================================================
// 유니티 Inspector 창은 기본적으로 public 변수와 [SerializeField] 변수만 보여줘.
// 하지만 CurrentHP처럼 게임 중에만 의미있는 값이나
// "피격" 같은 테스트 버튼은 기본 인스펙터로 추가할 수 없어.
//
// [CustomEditor(typeof(Warrior))]:
//   Warrior 컴포넌트의 Inspector를 이 클래스가 대신 그린다는 뜻.
//   이 클래스가 없으면 유니티 기본 Inspector가 사용돼.
//
// Editor 스크립트는 게임 빌드에 포함되지 않아! (에디터에서만 실행됨)
// ============================================================
[CustomEditor(typeof(Warrior))]
public class BasePlayerEditor : Editor
{
    // OnInspectorGUI: Inspector 창이 그려질 때마다 호출되는 함수
    // 여기서 GUI 요소들을 배치해서 커스텀 Inspector를 만들어
    public override void OnInspectorGUI()
    {
        // target: CustomEditor가 붙어있는 컴포넌트 (여기서는 Warrior → BasePlayer로 캐스팅)
        BasePlayer basePlayer = (BasePlayer)target;

        // 기본 Inspector를 그대로 먼저 그려줌 (기존 변수들 유지)
        DrawDefaultInspector();

        // IntField: 정수값을 입력받는 필드를 Inspector에 추가
        // "MaxHP" 레이블로 표시되고, 값을 변경하면 playerStats.maxHP에 반영
        basePlayer.playerStats.maxHP = EditorGUILayout.IntField("MaxHP", basePlayer.playerStats.maxHP);

        // LabelField: 읽기 전용 텍스트 레이블 (게임 실행 중 현재 HP 확인용)
        EditorGUILayout.LabelField("Current HP", basePlayer.CurrentHP.ToString());

        // GUILayout.Button: Inspector에 버튼 추가
        // 버튼이 클릭되면 if 블록 안의 코드 실행
        if (GUILayout.Button("피격"))
        {
            // 테스트용: 버튼 클릭 시 10 데미지를 플레이어에게 줌
            basePlayer.TakeDamage(10);
        }

        if (GUILayout.Button("초기화"))
        {
            // 테스트용: HP를 최대값으로 되돌림
            basePlayer.CurrentHP = basePlayer.playerStats.maxHP;
        }
    }
}
