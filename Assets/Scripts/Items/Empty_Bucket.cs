public class Empty_Bucket : Item
{
    protected override void InteractRight(PlayerInstance player, Location loc, bool firstFrameDown)
    {
        base.InteractRight(player, loc, firstFrameDown);
        
        Block block = loc.GetBlock();
        Material clickedMaterial = loc.GetMaterial();
        Material newBucketMaterial;
        
        if (clickedMaterial == Material.Water)
            newBucketMaterial = Material.Water_Bucket;
        else if (clickedMaterial == Material.Lava)
            newBucketMaterial = Material.Lava_Bucket;
        else return;

        if (block.GetData().GetTag("source_block") != "true")
            return;

        //Get player inventory
        Player playerEntity = player.playerEntity;
        PlayerInventory inv = playerEntity.GetInventoryHandler().GetInventory();
        
        //Remove one empty bucket
        ItemStack currentBucket = inv.GetSelectedItem();
        currentBucket.Amount--;
        inv.SetItem(inv.selectedSlot, currentBucket);
        
        //Give one filled bucket
        ItemStack newBucket = new ItemStack(newBucketMaterial, 1);
        inv.AddItem(newBucket);

        //Remove liquid block
        loc.SetMaterial(Material.Air).Tick();
    }
}