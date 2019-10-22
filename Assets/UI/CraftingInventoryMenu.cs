using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingInventoryMenu : ContainerInventoryMenu
{
    public override void OnClickSlot(int slotIndex, int clickType)
    {
        if (slotIndex == playerInventory.baseInventorySize + ((CraftingInventory)inventory).getCraftingResultSlot())
        {
            OnClickCraftingResultSlot(slotIndex, clickType);
            return;
        }

        base.OnClickSlot(slotIndex, clickType);
    }

    public virtual void OnClickCraftingResultSlot(int slotIndex, int clickType)
    {
        if (((CraftingInventory)inventory).getItem(((CraftingInventory)inventory).getCraftingResultSlot()).material == Material.Air)
        {
            return;
        }
        if (((CraftingInventory)inventory).getItem(((CraftingInventory)inventory).getCraftingResultSlot()).material != pointerSlot.item.material && pointerSlot.item.material != Material.Air)
        {
            return;
        }

        pointerSlot.item.material = ((CraftingInventory)inventory).getItem(((CraftingInventory)inventory).getCraftingResultSlot()).material;
        pointerSlot.item.amount += ((CraftingInventory)inventory).getItem(((CraftingInventory)inventory).getCraftingResultSlot()).amount;
        ((CraftingInventory)inventory).setItem(((CraftingInventory)inventory).getCraftingResultSlot(), new ItemStack());

        foreach (ItemStack craftingItem in ((CraftingInventory)inventory).getCraftingTable())
        {
            if (craftingItem.amount > 0)
                craftingItem.amount--;
        }
    }
}
