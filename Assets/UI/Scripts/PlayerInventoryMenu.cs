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
        PlayerInventory inv = ((PlayerInventory) Inventory.Get(inventoryIds[0]));
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
        PlayerInventory inv = ((PlayerInventory) Inventory.Get(inventoryIds[0]));
        
        if (inventoryIndex == 0 && slotIndex >= inv.GetFirstArmorSlot() && slotIndex < inv.GetFirstArmorSlot() + 4)
        {
            OnClickArmorSlot(slotIndex, clickType);
            return;
        }
        
        if (inventoryIndex == 0 && slotIndex == inv.GetCraftingResultSlot())
        {
            OnClickCraftingResultSlot(slotIndex, clickType);
            return;
        }

        base.OnClickSlot(inventoryIndex, slotIndex, clickType);
    }

    [Command(requiresAuthority = false)]
    public virtual void OnClickCraftingResultSlot(int slotIndex, int clickType)
    {
        PlayerInventory inv = ((PlayerInventory) Inventory.Get(inventoryIds[0]));
        
        if (inv.GetItem((inv.GetCraftingResultSlot())).material == Material.Air)
            return;
        
        if (inv.GetItem(inv.GetCraftingResultSlot()).material != pointerItem.material &&
            pointerItem.material != Material.Air)
            return;

        pointerItem.material = inv.GetItem(inv.GetCraftingResultSlot()).material;
        pointerItem.amount += inv.GetItem(inv.GetCraftingResultSlot()).amount;
        inv.SetItem(inv.GetCraftingResultSlot(), new ItemStack());

        for (int slot = inv.GetFirstCraftingTableSlot(); slot < inv.GetFirstCraftingTableSlot() + 4; slot++)
        {
            ItemStack newItem = inv.GetItem(slot).Clone();
            newItem.amount--;
            
            inv.SetItem(slot, newItem);
        }
        
        UpdateInventory();
    }
    
    [Command(requiresAuthority = false)]
    public virtual void OnClickArmorSlot(int slotIndex, int clickType)
    {
        
    }
}