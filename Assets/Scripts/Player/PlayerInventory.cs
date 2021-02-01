using System;

[Serializable]
public class PlayerInventory : Inventory
{
    public int baseInventorySize = 36;
    public int selectedSlot;

    public PlayerInventory()
    {
        initialize(45, "Inventory");
    }

    public ItemStack getSelectedItem()
    {
        return getItem(selectedSlot);
    }

    public ItemStack[] getHotbar()
    {
        var hotbar = new ItemStack[9];

        for (var i = 0; i < 9; i++) hotbar[i] = getItem(i);

        return hotbar;
    }

    public ItemStack[] getArmor()
    {
        var armor = new ItemStack[4];

        for (var i = getFirstArmorSlot(); i <= 39; i++) armor[i] = getItem(i);

        return armor;
    }

    public ItemStack[] getCraftingTable()
    {
        var table = new ItemStack[4];

        for (var i = getFirstCraftingTableSlot(); i < getFirstCraftingTableSlot() + 4; i++) table[i - 40] = getItem(i);

        return table;
    }
    
    public int getFirstArmorSlot()
    {
        return 36;
    }

    public int getFirstCraftingTableSlot()
    {
        return 40;
    }

    public int getCraftingResultSlot()
    {
        return 44;
    }

    public override void UpdateMenuStatus()
    {
        PlayerInventoryMenu invMenu = (PlayerInventoryMenu)GetInventoryMenu();
        invMenu.active = open;
        InventoryMenu.playerInventory = this;
    }

    public override InventoryMenu GetInventoryMenu()
    {
        return InventoryMenuManager.instance.playerInventoryMenu;
    }

    public override void Close()
    {
        var i = 0;
        foreach (var index in getCraftingTable())
        {
            index.Drop(holder, true);

            items[getFirstCraftingTableSlot() + i] = new ItemStack();
            i++;
        }

        base.Close();
    }
}