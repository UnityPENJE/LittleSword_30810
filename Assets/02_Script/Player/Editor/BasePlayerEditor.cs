using UnityEngine;
using LittleSword.Player;
using UnityEditor;

[CustomEditor(typeof(Warrior))]
public class BasePlayerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        BasePlayer basePlayer = (BasePlayer)target;
        DrawDefaultInspector();
        basePlayer.playerStats.maxHP = EditorGUILayout.IntField("MaxHP", basePlayer.playerStats.maxHP);
        EditorGUILayout.LabelField("Current HP", basePlayer.CurrentHP.ToString());
        if (GUILayout.Button("피격"))
        {
            basePlayer.TakeDamage(10);
        }

        if (GUILayout.Button("초기화"))
        {
            basePlayer.CurrentHP = basePlayer.playerStats.maxHP;
        }


         
    }
}
