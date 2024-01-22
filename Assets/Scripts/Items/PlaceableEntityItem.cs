public class PlaceableEntityItem : Item
{
    public virtual string entityType { get; } = "None Assigned";

    protected override void InteractRight(PlayerInstance player, Location loc, bool firstFrameDown)
    {
        base.InteractRight(player, loc, firstFrameDown);

        if (loc.GetMaterial() == Material.Air)
        {
            //Spawn Entity
            Entity entity = Entity.Spawn(entityType);
            entity.Teleport(loc);

            player.playerEntity.GetInventoryHandler().GetInventory().ConsumeSelectedItem();
        }
    }
}