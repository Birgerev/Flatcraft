public class Eye_Of_Ender : Item
{
    protected override void InteractRight(PlayerInstance player, Location loc, bool firstFrameDown)
    {
        base.InteractRight(player, loc, firstFrameDown);

        /*TODO enderportal place
        if (loc.GetMaterial() == Material.End_Portal_Frame)
        {
            return;
        }*/
        
        Entity entity = Entity.Spawn("EnderEye");
        entity.Teleport(loc);
        //TODO set owner

        //Remove player item
        player.playerEntity.GetInventoryHandler().GetInventory().ConsumeSelectedItem();
    }
}