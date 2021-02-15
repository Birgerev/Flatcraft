public class PlayerInventoryMenu : InventoryMenu
{
    private CraftingRecepie curRecepie;

    public override void UpdateInventory()
    {
        base.UpdateInventory();

        CheckCraftingRecepies();
    }

    public override void OnClickSlot(int inventory, int slotIndex, int clickType)
    {
        if (inventory == 0 && slotIndex >= ((PlayerInventory) inventories[0]).getFirstArmorSlot() && slotIndex < ((PlayerInventory) inventories[0]).getFirstArmorSlot() + 4)
        {
            OnClickArmorSlot(slotIndex, clickType);
            base.OnClickSlot(inventory, slotIndex, -1);
            return;
        }
        
        if (inventory == 0 && slotIndex == ((PlayerInventory) inventories[0]).getCraftingResultSlot())
        {
            OnClickCraftingResultSlot(slotIndex, clickType);
            base.OnClickSlot(inventory, slotIndex, -1);
            return;
        }

        base.OnClickSlot(inventory, slotIndex, clickType);
    }

    public void CheckCraftingRecepies()
    {
        CraftingRecepie newRecepie = CraftingRecepie.FindRecepieByItems(((PlayerInventory) inventories[0]).getCraftingTable());

        if (curRecepie != newRecepie)
            ScheduleUpdateInventory();

        curRecepie = newRecepie;

        if (newRecepie == null)
        {
            inventories[0].setItem(((PlayerInventory) inventories[0]).getCraftingResultSlot(), new ItemStack());
            return;
        }

        inventories[0].setItem(((PlayerInventory) inventories[0]).getCraftingResultSlot(), newRecepie.result);
    }

    public virtual void OnClickCraftingResultSlot(int slotIndex, int clickType)
    {
        if (inventories[0].getItem(((PlayerInventory) inventories[0]).getCraftingResultSlot()).material == Material.Air)
            return;
        if (inventories[0].getItem(((PlayerInventory) inventories[0]).getCraftingResultSlot()).material != pointerSlot.item.material &&
            pointerSlot.item.material != Material.Air)
            return;

        pointerSlot.item.material = inventories[0].getItem(((PlayerInventory) inventories[0]).getCraftingResultSlot()).material;
        pointerSlot.item.amount += inventories[0].getItem(((PlayerInventory) inventories[0]).getCraftingResultSlot()).amount;
        inventories[0].setItem(((PlayerInventory) inventories[0]).getCraftingResultSlot(), new ItemStack());

        foreach (var craftingItem in ((PlayerInventory) inventories[0]).getCraftingTable())
            if (craftingItem.amount > 0)
                craftingItem.amount--;

        CheckCraftingRecepies();
    }
    
    public virtual void OnClickArmorSlot(int slotIndex, int clickType)
    {
        
    }
}