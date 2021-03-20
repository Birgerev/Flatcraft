using System;
using UnityEngine;

public class InventoryContainer : Block
{
    public override void Initialize()
    {
        base.Initialize();

        if (!GetData().HasTag("inventoryId"))
        {
            Inventory inv = NewInventory();
            location.SetData(GetData().SetTag("inventoryId", inv.id.ToString()));
        }
    }

    public virtual Inventory NewInventory()
    {
        return Inventory.Create("Inventory", 0, "inventory name not set");
    }

    public override void Break(bool drop)
    {
        GetInventory().DropAll(location);
    
        //TODO delete inventory when break block

        base.Break(drop);
    }

    public override void Interact(PlayerInstance player)
    {
        base.Interact(player);

        GetInventory().Open(player);
    }

    public Inventory GetInventory()
    {
        int invId = int.Parse(GetData().GetTag("inventoryId"));
        return Inventory.Get(invId);
    }
}