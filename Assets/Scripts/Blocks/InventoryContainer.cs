using System;

public class InventoryContainer : Block
{
    public override void ServerInitialize()
    {
        base.ServerInitialize();

        //Create inventory if none has been assigned
        if (!GetData().HasTag("inventoryId"))
        {
            Inventory inv = NewInventory();
            location.SetData(GetData().SetTag("inventoryId", inv.id.ToString()));
        }
        //Fill with items if we have a loottable
        if (GetData().HasTag("loottable"))
        {
            string loottableName = GetData().GetTag("loottable");
            Loottable loottable = Loottable.Load(loottableName);
            Inventory inv = GetInventory();
            Random random = new Random();
            foreach (ItemStack item in loottable.GetRandomItems())
            {
                inv.SetItem(random.Next(0, inv.size), item);
            }
            location.SetData(GetData().RemoveTag("loottable"));
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