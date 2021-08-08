public class Empty_Bucket : Item
{
    public override string texture { get; set; } = "item_empty_bucket";
    
    protected override void InteractRight(PlayerInstance player, Location loc, bool firstFrameDown)
    {
        Block block = loc.GetBlock();
        Material clickedMaterial = loc.GetMaterial();
        Material newBucketMaterial = Material.Air;
        if (clickedMaterial == Material.Water)
        {
            newBucketMaterial = Material.Water_Bucket;
        }
        else if (clickedMaterial == Material.Lava)
        {
            newBucketMaterial = Material.Lava_Bucket;
        }
        else
        {
            base.InteractRight(player, loc, firstFrameDown);
            return;
        }

        if (block.GetData().GetTag("source_block") != "true")
        {
            base.InteractRight(player, loc, firstFrameDown);
            return;
        }

        //Get player inventory
        Player playerEntity = player.playerEntity.GetComponent<Player>();
        PlayerInventory inv = playerEntity.GetInventory();
        
        //Remove one empty bucket
        ItemStack currentBucket = inv.GetSelectedItem();
        currentBucket.amount--;
        inv.SetItem(inv.selectedSlot, currentBucket);
        
        //Give one filled bucket
        ItemStack newBucket = new ItemStack(newBucketMaterial, 1);
        inv.AddItem(newBucket);

        //Remove liquid block
        loc.SetMaterial(Material.Air).Tick();

        base.InteractRight(player, loc, firstFrameDown);
    }
}