using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerInventory : Inventory
{
    public int selectedSlot = 0;
    public int baseInventorySize = 36;

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
        ItemStack[] hotbar = new ItemStack[9];

        for (int i = 0; i < 9; i++)
        {
            hotbar[i] = getItem(i);
        }

        return hotbar;
    }

    public ItemStack[] getArmor()
    {
        ItemStack[] armor = new ItemStack[4];

        for (int i = 36; i <= 39; i++)
        {
            armor[i] = getItem(i);
        }

        return armor;
    }

    public ItemStack[] getCraftingTable()
    {
        ItemStack[] table = new ItemStack[4];

        for (int i = getFirstCraftingTableSlot(); i < getFirstCraftingTableSlot()+4; i++)
        {
            table[i-40] = getItem(i);
        }

        return table;
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
        PlayerInventoryMenu inventory = InventoryMenuManager.instance.playerInventoryMenu;
        inventory.active = open;

        InventoryMenu.playerInventory = this;
    }

    public override void Close()
    {
        int i = 0;
        foreach (ItemStack index in getCraftingTable())
        {
            index.Drop(holder, true);

            items[getFirstCraftingTableSlot() + i] = new ItemStack();
            i++;
        }

        base.Close();
    }
}
