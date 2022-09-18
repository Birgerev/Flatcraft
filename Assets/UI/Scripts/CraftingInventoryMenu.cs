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
        ItemStack resultItem = inv.GetItem(inv.GetCraftingResultSlot());
        ItemStack newPointerItem = pointerItem;

        if (resultItem.material == Material.Air)
            return;
        if (resultItem.material != pointerItem.material &&
            pointerItem.material != Material.Air)
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