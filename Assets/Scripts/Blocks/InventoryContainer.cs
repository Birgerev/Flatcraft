public class InventoryContainer : Block
{
    public override void ServerInitialize()
    {
        base.ServerInitialize();

        if (!GetData().HasTag("inventoryId"))
        {
            Inventory inv = NewInventory();
            location.SetData(GetData().SetTag("inventoryId", inv.id.ToString()));
        }

        GetInventory();
    }

    public virtual Inventory NewInventory()
    {
        return Inventory.Create("Inventory", 0, "inventory name not set");
    }

    public override void Break(bool drop)
    {
        GetInventory().DropAll(location);
        GetInventory().Delete();

        base.Break(drop);
    }

    public override void Interact(PlayerInstance player)
    {
        base.Interact(player);

        GetInventory().Open(player);
        GetInventory().holder = location;
    }

    public Inventory GetInventory()
    {
        int invId = int.Parse(GetData().GetTag("inventoryId"));
        return Inventory.Get(invId);
    }
}