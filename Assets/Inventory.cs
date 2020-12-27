using System;
using UnityEngine;

[Serializable]
public class Inventory
{
    public static int MaxStackSize = 64;
    public static bool anyOpen;

    public Location holder;
    public ItemStack[] items;
    public string name;

    public bool open;

    public int size;

    public Inventory()
    {
        initialize(27, "Inventory");
    }

    public Inventory(int size)
    {
        initialize(size, "Inventory");
    }

    public Inventory(int size, string name)
    {
        initialize(size, name);
    }

    public virtual void initialize(int size, string name)
    {
        this.size = size;
        Clear();
        this.name = name;
    }

    public void setItem(int slot, ItemStack item)
    {
        if (item.amount <= 0)
        {
            items[slot] = new ItemStack();
            return;
        }
        if (item.amount > MaxStackSize)
        {
            item.amount = MaxStackSize;
        }


        items[slot] = item;
    }

    public ItemStack getItem(int slot)
    {
        if (slot < size)
            return items[slot];
        return null;
    }

    public void Clear()
    {
        items = new ItemStack[size];
        for (var i = 0; i < size; i++) items[i] = new ItemStack();
    }

    public bool AddItem(ItemStack item)
    {
        foreach (var invItem in items)
            if (invItem.material == item.material && invItem.amount + item.amount <= MaxStackSize)
            {
                invItem.amount += item.amount;
                return true;
            }

        for (int slot = 0; slot < items.Length; slot++)
        {
            if (getItem(slot).material == Material.Air)
            {
                setItem(slot, item);
                return true;
            }
        }

        return false;
    }

    public ItemStack GetItemOfMaterial(Material mat)
    {
        foreach (var item in items)
            if (item.material == mat)
                return item;
        return null;
    }

    public bool ContainsAtLeast(Material mat, int amount)
    {
        var amountOfMaterial = 0;
        foreach (var item in items)
        {
            if (item.material == mat) amountOfMaterial += item.amount;

            if (amountOfMaterial >= amount)
                return true;
        }

        return false;
    }

    public bool Contains(Material mat)
    {
        foreach (var item in items)
            if (item.material == mat)
                return true;
        return false;
    }

    public bool Contains(Material mat, int amount)
    {
        foreach (var item in items)
            if (item.material == mat && item.amount == amount)
                return true;
        return false;
    }

    public void DropAll(Location dropPosition)
    {
        foreach (var item in items)
        {
            item.Drop(dropPosition + new Location(0, 1), true);
        }

        items = new ItemStack[size];
    }

    public virtual void Open(Location holder)
    {
        this.holder = holder;

        Cursor.visible = true;
        open = true;
        anyOpen = true;
        UpdateMenuStatus();
    }

    public virtual void Close()
    {
        Cursor.visible = false;
        open = false;
        anyOpen = false;
        UpdateMenuStatus();
    }

    public virtual void UpdateMenuStatus()
    {
        var inventory = InventoryMenuManager.instance.containerInventoryMenu;
        inventory.active = open;
        inventory.inventory = this;
        ContainerInventoryMenu invMenu = (ContainerInventoryMenu)GetInventoryMenu();
        invMenu.active = open;
        invMenu.inventory = this;
    }

    public virtual InventoryMenu GetInventoryMenu()
    {
        return InventoryMenuManager.instance.containerInventoryMenu;
    }
}