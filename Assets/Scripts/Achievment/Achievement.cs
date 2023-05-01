using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class Achievement : MonoBehaviour
{
    public string achievementId;

    protected bool performTracking = true;
    
    private void Start()
    {
        //No need to track achievement if we have it
        if (HasAchievement())
            performTracking = false;
    }

    [ContextMenu("Grant Achievement")]
    public void GrantAchievement()
    {
        SteamUserStats.SetAchievement(achievementId);
    }
    
    [ContextMenu("Revoke Achievement")]
    public void ResetAchievement()
    {
        SteamUserStats.ClearAchievement(achievementId);
    }
    
    [ContextMenu("Reset All Achievements")]
    public void ResetAchievements()
    {
        SteamUserStats.ResetAllStats(true);
    }
    
    [ContextMenu("Has Achievement?")]
    public bool HasAchievement()
    {
        SteamUserStats.GetAchievement(achievementId, out bool hasAchievement);
        Debug.Log(achievementId + " : " + hasAchievement);

        return hasAchievement;
    }
}
