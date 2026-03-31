using UnityEngine;
using LittleSword.Enemy;
using LittleSword.Enemy.FSM;
using UnityEditor;

[CustomEditor(typeof(Enemy))]
public class EnemyEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Enemy enemy = (Enemy)target;

        DrawDefaultInspector();
        EditorGUILayout.Space(10);
        GUI.enabled = Application.isPlaying;
        EditorGUILayout.LabelField("현재 상태", enemy.CurrentStateName);

        EditorGUILayout.BeginHorizontal();

        if(GUILayout.Button("Idle 상태"))
        {
            enemy.ChangeState<Idlestate>();
        }
        if (GUILayout.Button("Chase 상태"))
        {
            enemy.ChangeState<ChaseState>();
        }
        if (GUILayout.Button("Attack 상태"))
        {
            enemy.ChangeState<AttackState>();
        }

        EditorGUILayout.EndHorizontal();

        GUI.enabled = true;
    }
}
