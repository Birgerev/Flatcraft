using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryMenu : InventoryMenu
{
    public override bool wholePlayerInventory { get; } = true;
    private CraftingRecepie curRecepie;

    public override void FillSlots()
    {
        if (active)
        {
            CheckCraftingRecepies();
        }

        base.FillSlots();
    }

    public override void OnClickSlot(int slotIndex, int clickType)
    {
        if (slotIndex == playerInventory.getCraftingResultSlot())
        {
            OnClickCraftingResultSlot(slotIndex, clickType);
            return;
        }

        base.OnClickSlot(slotIndex, clickType);
    }


    public void CheckCraftingRecepies()
    {
        curRecepie = CraftingRecepie.FindRecepieByItems(playerInventory.getCraftingTable());

        if (curRecepie == null)
        {
            playerInventory.setItem(playerInventory.getCraftingResultSlot(), new ItemStack());
            return;
        }

        playerInventory.setItem(playerInventory.getCraftingResultSlot(), curRecepie.result);
    }

    public virtual void OnClickCraftingResultSlot(int slotIndex, int clickType)
    {
        if (playerInventory.getItem(playerInventory.getCraftingResultSlot()).material == Material.Air)
            return;
        if (playerInventory.getItem(playerInventory.getCraftingResultSlot()).material != pointerSlot.item.material && pointerSlot.item.material != Material.Air)
            return;

        pointerSlot.item.material = playerInventory.getItem(playerInventory.getCraftingResultSlot()).material;
        pointerSlot.item.amount += playerInventory.getItem(playerInventory.getCraftingResultSlot()).amount;
        playerInventory.setItem(playerInventory.getCraftingResultSlot(), new ItemStack());

        foreach (ItemStack craftingItem in playerInventory.getCraftingTable())
        {
            if (craftingItem.amount > 0)
                craftingItem.amount--;
        }

        CheckCraftingRecepies();
    }

}
