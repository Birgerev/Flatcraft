using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ItemHeldAchievment : Achievement
{
    public Material[] requiredItem;

    protected override void TrackingLoop()
    {
        PlayerInstance localPlayerInstance = PlayerInstance.localPlayerInstance;

        if (!localPlayerInstance)
            return;

        Player playerEntity = localPlayerInstance.playerEntity?.GetComponent<Player>();

        if (!playerEntity)
            return;

        PlayerInventory inventory = playerEntity.GetInventory();

        foreach (Material matAlternative in requiredItem)
        {
            if (inventory.Contains(matAlternative))
            {
                GrantAchievement();
                return;
            }
        }
    }
}
