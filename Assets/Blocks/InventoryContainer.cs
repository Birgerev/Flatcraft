using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryContainer : Block
{
    public Inventory inventory;
    public override bool autosave { get; } = true;
    public virtual System.Type inventoryType { get; } = typeof(Inventory);

    public override void FirstTick()
    {
        if (data.ContainsKey("inventory") && data["inventory"] != "")
        {
            inventory = (Inventory)JsonUtility.FromJson(data["inventory"], inventoryType);
        }
        else inventory = (Inventory)System.Activator.CreateInstance(inventoryType);

        base.FirstTick();
    }

    public override void Tick()
    {
        data["inventory"] = JsonUtility.ToJson(System.Convert.ChangeType(inventory, inventoryType));

        base.Tick();
    }

    public override void Break(bool drop)
    {
        inventory.DropAll(getPosition());

        base.Break(drop);
    }

    public override void Autosave()
    {
        data["inventory"] = JsonUtility.ToJson(System.Convert.ChangeType(inventory, inventoryType));

        if (inventory == null || inventory.open)
        {
            return;
        }
        
        base.Autosave();
    }

    public override void Interact()
    {
        base.Interact();

        inventory.Open(getPosition());
    }
}
