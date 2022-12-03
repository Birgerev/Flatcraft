using Mirror;

public class PlayerInventoryMenu : InventoryMenu
{
    private CraftingRecipe _curRecipe;

    [Server]
    public override void UpdateInventory()
    {
        CheckCraftingRecepies();
        base.UpdateInventory();
    }

    public override void OpenPlayerInventory()
    {
    }

    [Server]
    public void CheckCraftingRecepies()
    {
        PlayerInventory inv = (PlayerInventory) Inventory.Get(inventoryIds[0]);
        CraftingRecipe newRecipe = CraftingRecipe.FindRecipeByItems(inv.GetCraftingTableItems());

        if (newRecipe == null)
        {
            inv.SetItem(inv.GetCraftingResultSlot(), new ItemStack());
            return;
        }

        inv.SetItem(inv.GetCraftingResultSlot(), newRecipe.result);

        _curRecipe = newRecipe;
    }

    [Client]
    public override void OnClickSlot(int inventoryIndex, int slotIndex, int clickType)
    {
        PlayerInventory inv = (PlayerInventory) Inventory.Get(inventoryIds[0]);

        if (inventoryIndex == 0 && slotIndex >= inv.GetFirstArmorSlot() && slotIndex < inv.GetFirstArmorSlot() + 4)
        {
            OnClickArmorSlot(slotIndex, clickType);
            return;
        }

        if (inventoryIndex == 0 && slotIndex == inv.GetCraftingResultSlot())
        {
            OnClickCraftingResultSlot(inventoryIndex, slotIndex, clickType);
            return;
        }

        base.OnClickSlot(inventoryIndex, slotIndex, clickType);
    }

    [Command(requiresAuthority = false)]
    public virtual void OnClickCraftingResultSlot(int inventoryIndex, int slotIndex, int clickType)
    {
        PlayerInventory inv = (PlayerInventory) Inventory.Get(inventoryIds[0]);
        ItemStack newPointerItem = pointerItem;
        ItemStack resultItem = inv.GetItem(inv.GetCraftingResultSlot());

        if (resultItem.material == Material.Air)
            return;
        if (resultItem.material != newPointerItem.material &&
            newPointerItem.material != Material.Air)
            return;
        if (newPointerItem.amount >= Inventory.MaxStackSize)
            return;

        //Set pointer material to result material
        newPointerItem.material = resultItem.material;

        while (resultItem.amount > 0 && //Keep moving items until result slot is empty
               newPointerItem.Amount < Inventory.MaxStackSize)  //Stop if pointer amount exceeds 64
        {
            newPointerItem.Amount += 1;
            resultItem.amount -= 1;
        }

        //Properly clear result slot if necessary
        if (resultItem.amount <= 0)
            resultItem = new ItemStack();
                
        //Apply item stack changes
        SetPointerItem(newPointerItem);
        inv.SetItem(inv.GetCraftingResultSlot(), resultItem);

        for (int slot = inv.GetFirstCraftingTableSlot(); slot < inv.GetFirstCraftingTableSlot() + 4; slot++)
        {
            ItemStack newCraftingSlotItem = inv.GetItem(slot);
            if(newCraftingSlotItem.material == Material.Air)
                continue;
            
            newCraftingSlotItem.Amount--;
            inv.SetItem(slot, newCraftingSlotItem);
        }

        UpdateInventory();
    }

    [Command(requiresAuthority = false)]
    public virtual void OnClickArmorSlot(int slotIndex, int clickType)
    {
    }
}