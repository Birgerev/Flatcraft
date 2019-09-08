using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerInventoryMenu : InventoryMenu
{
    public Inventory inventory;


    public override ItemStack getItem(int index)
    {
        if (index < playerInventory.baseInventorySize)
            return base.getItem(index);
        else if (index < playerInventory.baseInventorySize + inventory.size)
        {
            return inventory.getItem(index - playerInventory.baseInventorySize);
        }

        return new ItemStack();
    }

    public override void FillSlots()
    {
        base.FillSlots();
        if (active)
        {
            for (int i = playerInventory.baseInventorySize; i < playerInventory.baseInventorySize + inventory.size; i++)
            {
                getSlotObject(i).item = getItem(i);
            }
        }
    }
}
