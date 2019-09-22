using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ContainerInventoryMenu : InventoryMenu
{
    public Text inventoryTitle;

    public Inventory inventory;


    public override void SetTitle()
    {
        base.SetTitle();
        inventoryTitle.text = inventory.name;
    }

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

    public override void Close()
    {
        base.Close();
        inventory.Close();
    }

    public override void FillSlots()
    {
        base.FillSlots();
        if (active)
        {
            for (int i = playerInventory.baseInventorySize; i < playerInventory.baseInventorySize + inventory.size; i++)
            {
                if(getSlotObject(i) != null)
                    getSlotObject(i).item = getItem(i);
            }
        }
    }
}
