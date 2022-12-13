using Mirror;

public class CraftingInventoryMenu : ContainerInventoryMenu
{
    public override void UpdateInventory()
    {
        RefreshCraftingResultSlot();
        base.UpdateInventory();
    }

    [Server]
    public void RefreshCraftingResultSlot()
    {
        CraftingInventory inv = (CraftingInventory) Inventory.Get(inventoryIds[0]);
        //Try to find a matching recipe
        CraftingRecipe curRecipe = CraftingRecipe.FindRecipeByItems(inv.GetCraftingTableItems());

        //If no matching recipe was found, clear result slot
        if (curRecipe == null)
        {
            inv.SetItem(inv.GetCraftingResultSlot(), new ItemStack());
            return;
        }

        //Otherwise fill result slot
        inv.SetItem(inv.GetCraftingResultSlot(), curRecipe.result);
    }

    [Client]
    public override void OnClickSlot(int inventoryIndex, int slotIndex, ClickType clickType)
    {
        CraftingInventory inv = (CraftingInventory) Inventory.Get(inventoryIds[0]);

        if (inventoryIndex == 0 && slotIndex == inv.GetCraftingResultSlot())
        {
            OnClickCraftingResultSlot(clickType);
            return;
        }

        base.OnClickSlot(inventoryIndex, slotIndex, clickType);
    }

    [Command(requiresAuthority = false)]
    public virtual void OnClickCraftingResultSlot(ClickType clickType)
    {
        if (clickType == ClickType.ShiftClick)
            SlotAction_TransferResultSlotUntilEmpty();
        else
            SlotAction_GrabResultSlot();
    }
    
    [Server]
    public virtual void SlotAction_GrabResultSlot()
    {
        CraftingInventory inv = (CraftingInventory) Inventory.Get(inventoryIds[0]);
        ItemStack resultItem = inv.GetItem(inv.GetCraftingResultSlot());
        ItemStack newPointerItem = pointerItem;

        //Cancel if result slot is empty
        if (resultItem.material == Material.Air)
            return;
        //Cancel if pointer and result slot materials dont match
        if (resultItem.material != pointerItem.material &&
            pointerItem.material != Material.Air)
            return;
        //Cancel if pointer slot is full
        if (newPointerItem.amount >= Inventory.MaxStackSize)
            return;
        
        //Set pointer material to result material
        newPointerItem.material = resultItem.material;
        
        //Keep moving items until result slot is empty or if pointer amount exceeds 64
        while (resultItem.amount > 0 && newPointerItem.Amount < Inventory.MaxStackSize) 
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
    
    [Server]
    public virtual void SlotAction_TransferResultSlotUntilEmpty()
    {
        CraftingInventory craftingInventory = (CraftingInventory) Inventory.Get(inventoryIds[0]);
        PlayerInventory playerInventory = (PlayerInventory) Inventory.Get(inventoryIds[1]);

        while (true)
        {
            ItemStack resultItem = craftingInventory.GetItem(craftingInventory.GetCraftingResultSlot());
            
            //Stop transferring if result slot is empty
            if (resultItem.material == Material.Air)
                break;
            
            //Subtract items from recipe slots
            for (int i = 0; i <= 8; i++)
            {
                ItemStack newCraftingSlotItem = craftingInventory.GetItem(i);
                if(newCraftingSlotItem.material == Material.Air)
                    continue;
            
                newCraftingSlotItem.Amount--;
                craftingInventory.SetItem(i, newCraftingSlotItem);
            }
                
            //Clear result slot
            craftingInventory.SetItem(craftingInventory.GetCraftingResultSlot(), new ItemStack());
            
            //Add result item to player inventory
            playerInventory.AddItem(resultItem);
            
            //Refresh Crafting slot for next iteration
            RefreshCraftingResultSlot();
        }
        
        //Update Inventory
        UpdateInventory();
    }
}