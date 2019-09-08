using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingInventory : Inventory
{
    public CraftingInventory()
    {
        initialize(10, "Crafting");
    }
    
    public ItemStack[] getCraftingTable()
    {
        ItemStack[] table = new ItemStack[9];

        for (int i = 0; i <= 8; i++)
        {
            table[i] = getItem(i);
        }

        return table;
    }

    public int getCraftingResultSlot()
    {
        return 9;
    }

    public override void ToggleOpen()
    {
        CraftingInventoryMenu inventory = InventoryMenuManager.instance.craftingInventoryMenu;
        inventory.active = !inventory.active;
        inventory.inventory = this;
    }
}
