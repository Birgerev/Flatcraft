using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerInventory : Inventory
{
    public int selectedSlot = 0;

    public PlayerInventory()
    {
        initialize(36, "Player Inventory");
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
}
