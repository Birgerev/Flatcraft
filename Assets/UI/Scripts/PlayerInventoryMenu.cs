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

        if (inv.GetItem(inv.GetCraftingResultSlot()).material == Material.Air)
            return;

        if (inv.GetItem(inv.GetCraftingResultSlot()).material != newPointerItem.material &&
            newPointerItem.material != Material.Air)
            return;


        newPointerItem.material = inv.GetItem(inv.GetCraftingResultSlot()).material;
        newPointerItem.Amount += inv.GetItem(inv.GetCraftingResultSlot()).Amount;
        inv.SetItem(inv.GetCraftingResultSlot(), new ItemStack());
        SetPointerItem(newPointerItem);

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