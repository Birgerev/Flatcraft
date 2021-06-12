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

        ItemStack newPointerItem = inv.GetItem(inv.GetCraftingResultSlot()).Clone();
        newPointerItem.amount = pointerItem.amount + inv.GetItem(inv.GetCraftingResultSlot()).amount;
        SetPointerItem(newPointerItem);

        //Clear Crafting slot
        inv.SetItem(inv.GetCraftingResultSlot(), new ItemStack());

        for (int i = 0; i <= 8; i++)
        {
            ItemStack newCraftingSlotItem = inv.GetItem(i).Clone();
            newCraftingSlotItem.amount--;

            inv.SetItem(i, newCraftingSlotItem);
        }

        UpdateInventory();
    }
}