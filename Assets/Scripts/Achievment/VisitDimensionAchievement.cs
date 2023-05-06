using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisitDimensionAchievement : Achievement
{
    public Dimension trackedDimension;

    protected override void TrackingLoop()
    {
        PlayerInstance localPlayerInstance = PlayerInstance.localPlayerInstance;

        if (!localPlayerInstance)
            return;

        Player playerEntity = localPlayerInstance.playerEntity?.GetComponent<Player>();

        if (!playerEntity)
            return;
        
        if (playerEntity.Location.dimension == trackedDimension)
            GrantAchievement();
    }
}