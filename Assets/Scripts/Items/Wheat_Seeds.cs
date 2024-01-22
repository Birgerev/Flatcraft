public class Wheat_Seeds : PlaceableItem
{
    public override Material blockMaterial { get; } = Material.Wheat_Crop;
    
    protected override void InteractRight(PlayerInstance player, Location loc, bool firstFrameDown)
    {
        base.InteractRight(player, loc, firstFrameDown);

        if (!firstFrameDown) return;

        Material clickedMaterial = loc.GetMaterial();
        if (clickedMaterial != Material.Farmland_Dry && clickedMaterial != Material.Farmland_Wet) return;

        Location locAbove = loc + new Location(0, 1);
        if (locAbove.GetMaterial() != Material.Air) return;
        
        //Remove one filled bucket
        player.playerEntity.GetInventoryHandler().GetInventory().ConsumeSelectedItem();
        
        //Place liquid block
        locAbove.SetMaterial(blockMaterial).SetData(new BlockData("source_block=true")).Tick();
        
        base.InteractRight(player, loc, firstFrameDown);
    }
}