using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Steamworks;
using UnityEngine;

public class Achievement : MonoBehaviour
{
    public string achievementId;
    
    [Space]

    protected bool performTracking = true;
    protected virtual float TrackingLoopInterval => 2;

    protected virtual void Start()
    {
        //No need to track achievement if we already have it
        if (HasAchievement())
            performTracking = false;
        
        TrackingLoopCaller();
    }

    
    private async void TrackingLoopCaller()
    {
        while (true)
        {
            if(!performTracking)
                return;
            
            //Convert interval to milliseconds
            await Task.Delay((int)(TrackingLoopInterval * 1000));

            TrackingLoop();
        }
    }
    
    protected virtual void TrackingLoop(){}

    #region Achievement Status Handling
    
    [ContextMenu("Grant Achievement")]
    public void GrantAchievement()
    {
        SteamUserStats.SetAchievement(achievementId);
        Debug.Log("Granted achievement id: " + achievementId);
    }
    
    [ContextMenu("Revoke Achievement")]
    public void RevokeAchievement()
    {
        SteamUserStats.ClearAchievement(achievementId);
        Debug.Log("Revoked achievement id: " + achievementId);
    }

    public bool HasAchievement()
    {
        SteamUserStats.GetAchievement(achievementId, out bool hasAchievement);

        return hasAchievement;
    }
    
    [ContextMenu("Has Achievement?")]
    public void Debug_HasAchievementMessage()
    {
        Debug.Log("Has achievement '" + achievementId + "' : " + HasAchievement());
    }
        
    [ContextMenu("Reset All Achievements")]
    public void Debug_ResetAchievements()
    {
        //Wrapper function as we cant use context menu attribute on static method
        ResetAchievements();
    }
    
    public static void ResetAchievements()
    {
        SteamUserStats.ResetAllStats(true);
    }
    #endregion
}
