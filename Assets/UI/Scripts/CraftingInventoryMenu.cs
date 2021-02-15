public class CraftingInventoryMenu : ContainerInventoryMenu
{
    public override void OnClickSlot(int inventory, int slotIndex, int clickType)
    {
        if (inventory == 1 && slotIndex == ((CraftingInventory) inventories[1]).GetCraftingResultSlot())
        {
            OnClickCraftingResultSlot(inventory, slotIndex, clickType);
            base.OnClickSlot(inventory, slotIndex, -1);
            return;
        }

        base.OnClickSlot(inventory, slotIndex, clickType);
    }

    public virtual void OnClickCraftingResultSlot(int inventory, int slotIndex, int clickType)
    {
        if (inventories[1].getItem(((CraftingInventory) inventories[1]).GetCraftingResultSlot()).material == Material.Air) 
            return;
        if (inventories[1].getItem(((CraftingInventory) inventories[1]).GetCraftingResultSlot()).material != pointerSlot.item.material && 
            pointerSlot.item.material != Material.Air) 
            return;

        pointerSlot.item.material = inventories[1].getItem(((CraftingInventory) inventories[1]).GetCraftingResultSlot()).material;
        pointerSlot.item.amount += inventories[1].getItem(((CraftingInventory) inventories[1]).GetCraftingResultSlot()).amount;
        pointerSlot.item.data = inventories[1].getItem(((CraftingInventory) inventories[1]).GetCraftingResultSlot()).data;
        pointerSlot.item.durability = inventories[1].getItem(((CraftingInventory) inventories[1]).GetCraftingResultSlot()).durability;

        //Clear Crafting slot
        inventories[1].setItem(((CraftingInventory) inventories[1]).GetCraftingResultSlot(), new ItemStack());

        for (int i = 0; i <= 8; i++)
        {
            ItemStack newCraftingSlotItem = inventories[1].getItem(i).Clone();
            if (newCraftingSlotItem.amount > 0)
                newCraftingSlotItem.amount--;
            
            inventories[1].setItem(i, newCraftingSlotItem);
        }
    }
}