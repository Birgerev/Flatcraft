public class PlaceableEntityItem : Item
{
    public virtual string entityType { get; } = "PaintingEntity";

    protected override void InteractRight(PlayerInstance player, Location loc, bool firstFrameDown)
    {
        base.InteractRight(player, loc, firstFrameDown);

        if (loc.GetMaterial() == Material.Air)
        {
            //Spawn Entity
            Entity entity = Entity.Spawn(entityType);
            entity.Teleport(loc);

            //Remove player item
            PlayerInventory inv = player.playerEntity.GetComponent<Player>().GetInventory();
            ItemStack selectedItem = inv.GetSelectedItem();
            selectedItem.Amount--;
            inv.SetItem(inv.selectedSlot, selectedItem);
        }
    }
}