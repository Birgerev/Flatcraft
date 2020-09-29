public class CraftingInventory : Inventory
{
    public CraftingInventory()
    {
        initialize(10, "Crafting");
    }

    public ItemStack[] getCraftingTable()
    {
        var table = new ItemStack[9];

        for (var i = 0; i <= 8; i++) table[i] = getItem(i);

        return table;
    }

    public int getCraftingResultSlot()
    {
        return 9;
    }

    public override void UpdateMenuStatus()
    {
        var inventory = InventoryMenuManager.instance.craftingInventoryMenu;
        inventory.active = open;
        inventory.inventory = this;
    }

    public override void Close()
    {
        //Don't drop results slot
        items[getCraftingResultSlot()] = new ItemStack();

        DropAll(holder);

        base.Close();
    }
}