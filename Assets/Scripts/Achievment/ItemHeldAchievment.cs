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

        Player player = localPlayerInstance.playerEntity?.GetComponent<Player>();

        if (!player)
            return;

        PlayerInventory inventory = player.GetInventory();

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
