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
        //No need to track achievement if we already have it
        if (HasAchievement())
            performTracking = false;
    }

    #region Achievement Status Handling
    
    [ContextMenu("Grant Achievement")]
    public void GrantAchievement()
    {
        SteamUserStats.SetAchievement(achievementId);
    }
    
    [ContextMenu("Revoke Achievement")]
    public void RevokeAchievement()
    {
        SteamUserStats.ClearAchievement(achievementId);
    }

    public bool HasAchievement()
    {
        SteamUserStats.GetAchievement(achievementId, out bool hasAchievement);
        Debug.Log(achievementId + " : " + hasAchievement);

        return hasAchievement;
    }
    
    [ContextMenu("Has Achievement?")]
    public void Debug_HasAchievement()
    {
        Debug.Log("Has achievement '" + achievementId + "' : " + HasAchievement());
    }
        
    [ContextMenu("Reset All Achievements")]
    public static void ResetAchievements()
    {
        SteamUserStats.ResetAllStats(true);
    }
    #endregion
}
