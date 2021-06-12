using System;
using Mirror;

[Serializable]
public class PlayerInventory : Inventory
{
    [SyncVar] public int selectedSlot;

    [Server]
    public static Inventory CreatePreset()
    {
        return Create("PlayerInventory", 45, "Inventory");
    }

    public ItemStack GetSelectedItem()
    {
        return GetItem(selectedSlot);
    }

    public ItemStack[] GetHotbarItems()
    {
        ItemStack[] hotbar = new ItemStack[9];

        for (int i = 0; i < 9; i++)
            hotbar[i] = GetItem(i);

        return hotbar;
    }

    public ItemStack[] GetArmorItems()
    {
        ItemStack[] armor = new ItemStack[4];

        for (int i = GetFirstArmorSlot(); i <= 39; i++)
            armor[i] = GetItem(i);

        return armor;
    }

    public ItemStack[] GetCraftingTableItems()
    {
        ItemStack[] table = new ItemStack[4];

        for (int i = GetFirstCraftingTableSlot(); i < GetFirstCraftingTableSlot() + 4; i++)
            table[i - 40] = GetItem(i);

        return table;
    }

    [Server]
    public override void Close()
    {
        base.Close();

        for (int slot = GetFirstCraftingTableSlot(); slot < GetFirstCraftingTableSlot() + 4; slot++)
        {
            GetItem(slot).Drop(holder);
            SetItem(slot, new ItemStack());
        }
    }

    public int GetFirstArmorSlot()
    {
        return 36;
    }

    public int GetFirstCraftingTableSlot()
    {
        return 40;
    }

    public int GetCraftingResultSlot()
    {
        return 44;
    }
}