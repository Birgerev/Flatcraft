using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : InventoryContainer
{
    public static string default_texture = "block_chest";
    public override bool playerCollide { get; } = false;
    public override float breakTime { get; } = 6;

    public override Tool_Type propperToolType { get; } = Tool_Type.Axe;

    public override System.Type inventoryType { get; } = typeof(Inventory);

    public override void Tick(bool spread)
    {
        if (inventory == null)
        {
            base.Tick(spread);
            return;
        }

        inventory.name = "Chest";
        base.Tick(spread);
    }

    private Inventory getInventory()
    {
        return ((Inventory)inventory);
    }
}
