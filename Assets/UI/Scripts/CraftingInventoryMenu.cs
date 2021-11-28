using Mirror;

public class CraftingInventoryMenu : ContainerInventoryMenu
{
    public override void UpdateInventory()
    {
        CheckCraftingRecipes();
        base.UpdateInventory();
    }

    [Server]
    public void CheckCraftingRecipes()
    {
        CraftingInventory inv = (CraftingInventory) Inventory.Get(inventoryIds[0]);
        CraftingRecipe curRecipe = CraftingRecipe.FindRecipeByItems(inv.GetCraftingTableItems());

        if (curRecipe == null)
        {
            inv.SetItem(inv.GetCraftingResultSlot(), new ItemStack());
            return;
        }

        inv.SetItem(inv.GetCraftingResultSlot(), curRecipe.result);
    }

    [Client]
    public override void OnClickSlot(int inventoryIndex, int slotIndex, int clickType)
    {
        CraftingInventory inv = (CraftingInventory) Inventory.Get(inventoryIds[0]);

        if (inventoryIndex == 0 && slotIndex == inv.GetCraftingResultSlot())
        {
            OnClickCraftingResultSlot();
            return;
        }

        base.OnClickSlot(inventoryIndex, slotIndex, clickType);
    }

    [Command(requiresAuthority = false)]
    public virtual void OnClickCraftingResultSlot()
    {
        CraftingInventory inv = (CraftingInventory) Inventory.Get(inventoryIds[0]);

        if (inv.GetItem(inv.GetCraftingResultSlot()).material == Material.Air)
            return;
        if (inv.GetItem(inv.GetCraftingResultSlot()).material != pointerItem.material &&
            pointerItem.material != Material.Air)
            return;

        //Add items to pointer
        ItemStack newPointerItem = inv.GetItem(inv.GetCraftingResultSlot());
        int newAmount = pointerItem.Amount + inv.GetItem(inv.GetCraftingResultSlot()).Amount;
        //Return if new item amount exceeds max stack size (64)
        if (newAmount > Inventory.MaxStackSize)
            return;
        
        newPointerItem.Amount = pointerItem.Amount + inv.GetItem(inv.GetCraftingResultSlot()).Amount;
        SetPointerItem(newPointerItem);

        //Clear Crafting slot
        inv.SetItem(inv.GetCraftingResultSlot(), new ItemStack());

        //Remove items from recipe slots
        for (int i = 0; i <= 8; i++)
        {
            ItemStack newCraftingSlotItem = inv.GetItem(i);
            if(newCraftingSlotItem.material == Material.Air)
                continue;
            
            newCraftingSlotItem.Amount--;
            inv.SetItem(i, newCraftingSlotItem);
        }

        //Update Inventory
        UpdateInventory();
    }
}