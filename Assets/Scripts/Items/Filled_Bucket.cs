using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Filled_Bucket : Item
{
    public virtual Material bucketBlock { get; set; } = Material.Air;
    
    protected override void InteractRight(PlayerInstance player, Location loc, bool firstFrameDown)
    {
        base.InteractRight(player, loc, firstFrameDown);

        if (!firstFrameDown)
            return;

        Material clickedMaterial = loc.GetMaterial();

        if (clickedMaterial != Material.Air && loc.GetBlock().solid)
            return;

        //Get player inventory
        Player playerEntity = player.playerEntity.GetComponent<Player>();
        PlayerInventory inv = playerEntity.GetInventory();
        
        //Remove one filled bucket
        ItemStack currentBucket = inv.GetSelectedItem();
        currentBucket.Amount--;
        inv.SetItem(inv.selectedSlot, currentBucket);
        
        //Give one empty bucket
        ItemStack newBucket = new ItemStack(Material.Empty_Bucket, 1);
        inv.AddItem(newBucket);

        //Place liquid block
        loc.SetMaterial(bucketBlock).SetData(new BlockData("source_block=true")).Tick();

        base.InteractRight(player, loc, firstFrameDown);
    }
}
