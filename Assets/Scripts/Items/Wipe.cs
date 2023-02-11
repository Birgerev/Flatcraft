using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wipe : Item
{
    protected override void InteractRight(PlayerInstance player, Location loc, bool firstFrameDown)
    {
        base.InteractRight(player, loc, firstFrameDown);
        
        //Get player inventory
        Player playerEntity = player.playerEntity.GetComponent<Player>();
        PlayerInventory inv = playerEntity.GetInventory();
        
        //Remove one empty bucket
        inv.SetItem(inv.selectedSlot, new ItemStack());
        
        ChatManager.instance.AddMessagePlayer("Go on...   wipe", player);
    }
}
