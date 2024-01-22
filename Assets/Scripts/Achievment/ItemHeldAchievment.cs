using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ItemHeldAchievment : Achievement
{
    public Material[] materialCriteria;

    protected override void TrackingLoop()
    {
        PlayerInstance localPlayerInstance = PlayerInstance.localPlayerInstance;

        if (!localPlayerInstance)
            return;

        Player playerEntity = localPlayerInstance.playerEntity;

        if (!playerEntity)
            return;

        PlayerInventory inventory = playerEntity.GetInventoryHandler().GetInventory();

        if (inventory == null)
            return;

        foreach (Material matAlternative in materialCriteria)
        {
            if (inventory.Contains(matAlternative))
            {
                GrantAchievement();
                return;
            }
        }
    }
}
