using UnityEngine;
using LittleSword.Enemy;
using LittleSword.Enemy.FSM;
using UnityEditor;

// ============================================================
// EnemyEditor: 유니티 에디터에서 적 상태를 보고 직접 전환하는 커스텀 인스펙터
// ============================================================
// FSM(유한 상태 기계)을 개발할 때 가장 불편한 점은
// 특정 상태를 테스트하려면 게임을 실행해서 조건을 맞춰야 한다는 거야.
//
// EnemyEditor는 게임 실행 중에 Inspector에서 버튼 클릭만으로
// Idle, Chase, Attack 상태로 바로 전환할 수 있게 해줘!
// → 개발과 디버깅이 훨씬 편해짐
//
// [CustomEditor(typeof(Enemy))]:
//   Enemy 컴포넌트의 Inspector를 이 클래스가 대신 그림
// ============================================================
[CustomEditor(typeof(Enemy))]
public class EnemyEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // target을 Enemy 타입으로 캐스팅해서 Enemy의 함수에 접근
        Enemy enemy = (Enemy)target;

        // 기본 Inspector 항목들을 먼저 그려줌
        DrawDefaultInspector();

        // 버튼들 사이 여백 추가 (10픽셀)
        EditorGUILayout.Space(10);

        // GUI.enabled: false이면 아래 UI 요소들이 회색으로 비활성화됨
        // Application.isPlaying: 게임이 실행 중일 때만 true
        // → 상태 전환은 게임 실행 중에만 가능하도록 제한!
        GUI.enabled = Application.isPlaying;

        // 현재 상태 이름을 읽기 전용 레이블로 표시
        EditorGUILayout.LabelField("현재 상태", enemy.CurrentStateName);

        // BeginHorizontal/EndHorizontal: 사이에 있는 UI 요소들을 가로로 나란히 배치
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Idle 전환"))
        {
            enemy.ChangeState<Idlestate>();
        }
        if (GUILayout.Button("Chase 전환"))
        {
            enemy.ChangeState<ChaseState>();
        }
        if (GUILayout.Button("Attack 전환"))
        {
            enemy.ChangeState<AttackState>();
        }

        EditorGUILayout.EndHorizontal();

        // 버튼 그룹이 끝나면 다시 GUI를 활성화 상태로 되돌림
        GUI.enabled = true;
    }
}
