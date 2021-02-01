public class PlayerInventoryMenu : InventoryMenu
{
    private CraftingRecepie curRecepie;
    public override bool wholePlayerInventory { get; } = true;

    public override void UpdateInventory()
    {
        base.UpdateInventory();

        CheckCraftingRecepies();
    }

    public override void OnClickSlot(int slotIndex, int clickType)
    {
        if (slotIndex >= playerInventory.getFirstArmorSlot() && slotIndex < playerInventory.getFirstArmorSlot() + 4)
        {
            OnClickArmorSlot(slotIndex, clickType);
            base.OnClickSlot(-1, -1);
            return;
        }
        
        if (slotIndex == playerInventory.getCraftingResultSlot())
        {
            OnClickCraftingResultSlot(slotIndex, clickType);
            base.OnClickSlot(-1, -1);
            return;
        }

        base.OnClickSlot(slotIndex, clickType);
    }


    public void CheckCraftingRecepies()
    {
        CraftingRecepie newRecepie = CraftingRecepie.FindRecepieByItems(playerInventory.getCraftingTable());

        if (curRecepie != newRecepie)
            ScheduleUpdateInventory();

        curRecepie = newRecepie;

        if (newRecepie == null)
        {
            playerInventory.setItem(playerInventory.getCraftingResultSlot(), new ItemStack());
            return;
        }

        playerInventory.setItem(playerInventory.getCraftingResultSlot(), newRecepie.result);
    }

    public virtual void OnClickCraftingResultSlot(int slotIndex, int clickType)
    {
        if (playerInventory.getItem(playerInventory.getCraftingResultSlot()).material == Material.Air)
            return;
        if (playerInventory.getItem(playerInventory.getCraftingResultSlot()).material != pointerSlot.item.material &&
            pointerSlot.item.material != Material.Air)
            return;

        pointerSlot.item.material = playerInventory.getItem(playerInventory.getCraftingResultSlot()).material;
        pointerSlot.item.amount += playerInventory.getItem(playerInventory.getCraftingResultSlot()).amount;
        playerInventory.setItem(playerInventory.getCraftingResultSlot(), new ItemStack());

        foreach (var craftingItem in playerInventory.getCraftingTable())
            if (craftingItem.amount > 0)
                craftingItem.amount--;

        CheckCraftingRecepies();
    }
    
    public virtual void OnClickArmorSlot(int slotIndex, int clickType)
    {
        
    }
}