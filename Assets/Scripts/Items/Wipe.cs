using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wipe : Item
{
    protected override void InteractRight(PlayerInstance player, Location loc, bool firstFrameDown)
    {
        base.InteractRight(player, loc, firstFrameDown);
        
        //Remove one empty bucket
        ChatManager.instance.AddMessagePlayer("Go on...   wipe", player);
        player.playerEntity.GetInventoryHandler().GetInventory().ConsumeSelectedItem();
    }
}
