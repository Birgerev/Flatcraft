public class CraftingInventoryMenu : ContainerInventoryMenu
{
    public override void OnClickSlot(int slotIndex, int clickType)
    {
        if (slotIndex == playerInventory.baseInventorySize + ((CraftingInventory) inventory).GetCraftingResultSlot())
        {
            OnClickCraftingResultSlot(slotIndex, clickType);
            base.OnClickSlot(-1, -1);
            return;
        }

        base.OnClickSlot(slotIndex, clickType);
    }

    public virtual void OnClickCraftingResultSlot(int slotIndex, int clickType)
    {
        if (((CraftingInventory) inventory).getItem(((CraftingInventory) inventory).GetCraftingResultSlot()).material ==
            Material.Air) return;
        if (((CraftingInventory) inventory).getItem(((CraftingInventory) inventory).GetCraftingResultSlot()).material !=
            pointerSlot.item.material && pointerSlot.item.material != Material.Air) return;

        pointerSlot.item.material = ((CraftingInventory) inventory)
            .getItem(((CraftingInventory) inventory).GetCraftingResultSlot()).material;
        pointerSlot.item.amount += ((CraftingInventory) inventory)
            .getItem(((CraftingInventory) inventory).GetCraftingResultSlot()).amount;
        pointerSlot.item.data = ((CraftingInventory) inventory)
            .getItem(((CraftingInventory) inventory).GetCraftingResultSlot()).data;
        pointerSlot.item.durability = ((CraftingInventory) inventory)
            .getItem(((CraftingInventory) inventory).GetCraftingResultSlot()).durability;

        //Clear Crafting slot
        ((CraftingInventory) inventory).setItem(((CraftingInventory) inventory).GetCraftingResultSlot(),
            new ItemStack());

        foreach (var craftingItem in ((CraftingInventory) inventory).GetCraftingTable())
            if (craftingItem.amount > 0)
                craftingItem.amount--;
    }
}