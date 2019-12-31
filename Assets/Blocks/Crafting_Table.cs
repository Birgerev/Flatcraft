using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crafting_Table : InventoryContainer
{
    public static string default_texture = "block_crafting_table";
    public override bool playerCollide { get; } = false;
    public override float breakTime { get; } = 3;

    public override Tool_Type propperToolType { get; } = Tool_Type.Axe;

    public override System.Type inventoryType { get; } = typeof(CraftingInventory);

    public override void Tick(bool spread)
    {
        CheckCraftingRecepies();

        base.Tick(spread);
    }
    
    public void CheckCraftingRecepies()
    {
        CraftingRecepie curRecepie = CraftingRecepie.FindRecepieByItems(getInventory().getCraftingTable());

        if (curRecepie == null)
        {
            getInventory().setItem(getInventory().getCraftingResultSlot(), new ItemStack());
            return;
        }

        getInventory().setItem(getInventory().getCraftingResultSlot(), curRecepie.result);
    }

    public override void Interact()
    {
        CraftingInventory newInv = new CraftingInventory();
        inventory = newInv;
        newInv.Open(position);
    }

    private CraftingInventory getInventory()
    {
        return ((CraftingInventory)inventory);
    }
}
