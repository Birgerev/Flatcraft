using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEditor;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void ResetAchievements()
    {
        SteamUserStats.ResetAllStats(true);
    }
}

[CustomEditor(typeof(AchievementManager))]
[CanEditMultipleObjects]
public class AchievementManagerEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Reset Achievements", EditorStyles.miniButton))
        {
            ((AchievementManager)target).ResetAchievements();
        }
        
        DrawDefaultInspector ();
    }
}