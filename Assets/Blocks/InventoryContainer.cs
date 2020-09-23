using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryContainer : Block
{
    public Inventory inventory;
    public override bool autosave { get; } = true;
    public virtual System.Type inventoryType { get; } = typeof(Inventory);
    public override bool autoTick { get; } = true;

    public override void Initialize()
    {
        base.Initialize();

        if (data.GetData("inventory") != "")
        {
            inventory = (Inventory)JsonUtility.FromJson(data.GetData("inventory"), inventoryType);
        }
        else inventory = (Inventory)System.Activator.CreateInstance(inventoryType);
    }

    public override void Tick()
    {
        data.SetData("inventory", JsonUtility.ToJson(System.Convert.ChangeType(inventory, inventoryType)));

        base.Tick();
    }

    public override void Break(bool drop)
    {
        inventory.DropAll(location);

        base.Break(drop);
    }

    public override void Autosave()
    {
        data.SetData("inventory", JsonUtility.ToJson(System.Convert.ChangeType(inventory, inventoryType)));

        if (inventory == null || inventory.open)
        {
            return;
        }
        
        base.Autosave();
    }

    public override void Interact()
    {
        base.Interact();

        inventory.Open(location);
    }
}
