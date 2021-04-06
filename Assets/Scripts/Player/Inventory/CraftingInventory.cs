using Mirror;

public class CraftingInventory : Inventory
{
    [Server]
    public static Inventory CreatePreset()
    {
        return Create("CraftingInventory", 10, "Crafting");
    }
    
    public ItemStack[] GetCraftingTableItems()
    {
        var table = new ItemStack[9];

        for (var i = 0; i <= 8; i++) 
            table[i] = GetItem(i);

        return table;
    }

    [Server]
    public override void Close()
    {
        base.Close();
        
        foreach (ItemStack item in GetCraftingTableItems())
        {
            item.Drop(holder);
        }
        
        Clear();
    }

    public int GetCraftingResultSlot()
    {
        return 9;
    }
}