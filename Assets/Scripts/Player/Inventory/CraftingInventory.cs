public class CraftingInventory : Inventory
{
    public CraftingInventory()
    {
        initialize(10, "Crafting");
    }

    public ItemStack[] GetCraftingTable()
    {
        var table = new ItemStack[9];

        for (var i = 0; i <= 8; i++) 
            table[i] = getItem(i);

        return table;
    }

    public int GetCraftingResultSlot()
    {
        return 9;
    }

    public override void UpdateMenuStatus()
    {
        CraftingInventoryMenu invMenu = (CraftingInventoryMenu)GetInventoryMenu();
        CraftingInventoryMenu.inventories[1] = this;
        invMenu.active = open;
    }

    public override InventoryMenu GetInventoryMenu()
    {
        return InventoryMenuManager.instance.craftingInventoryMenu;
    }

    public override void Close()
    {
        //Don't drop results slot
        items[GetCraftingResultSlot()] = new ItemStack();

        DropAll(holder);

        base.Close();
    }
}