using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisitDimensionAchievement : Achievement
{
    public Dimension trackedDimension;

#if !DISABLESTEAMWORKS
    protected override void TrackingLoop()
    {
        PlayerInstance localPlayerInstance = PlayerInstance.localPlayerInstance;

        if (!localPlayerInstance)
            return;

        Player playerEntity = localPlayerInstance.playerEntity;

        if (!playerEntity)
            return;
        
        if (playerEntity.Location.dimension == trackedDimension)
            GrantAchievement();
    }
#endif
}