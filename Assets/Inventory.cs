using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Inventory
{
    public int size;
    public ItemStack[] items;
    public string name;

    public Vector2Int holder;

    public bool open;

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
        items[slot] = item;
    }

    public ItemStack getItem(int slot)
    {
        if (slot < size)
            return items[slot];
        else
            return null;
    }

    public void Clear()
    {
        items = new ItemStack[size];
        for (int i = 0; i < size; i++)
        {
            items[i] = new ItemStack(Material.Air, 0);
        }
    }

    public bool AddItem(ItemStack item)
    {
        foreach (ItemStack invItem in items)
        {
            if (invItem.material == item.material && invItem.data == item.data)
            {
                invItem.amount += item.amount;
                return true;
            }
        }

        foreach (ItemStack invItem in items)
        {
            if(invItem == null || invItem.material == Material.Air)
            {
                invItem.material = item.material;
                invItem.amount = item.amount;
                invItem.data = item.data;
                return true;
            }
        }

        return false;
    }

    public ItemStack GetItemOfMaterial(Material mat)
    {
        foreach (ItemStack item in items)
        {
            if(item.material == mat)
                return item;
        }
        return null;
    }

    public bool ContainsAtLeast(Material mat, int amount)
    {
        int amountOfMaterial = 0;
        foreach (ItemStack item in items)
        {
            if (item.material == mat)
            {
                amountOfMaterial += item.amount;
            }

            if (amountOfMaterial >= amount)
                return true;
        }
        return false;
    }

    public bool Contains(Material mat)
    {
        foreach (ItemStack item in items)
        {
            if (item.material == mat)
                return true;
        }
        return false;
    }

    public bool Contains(Material mat, int amount)
    {
        foreach (ItemStack item in items)
        {
            if (item.material == mat && item.amount == amount)
                return true;
        }
        return false;
    }

    public void DropAll(Vector2Int dropPosition)
    {
        foreach (ItemStack item in items)
        {
            dropPosition = dropPosition + new Vector2Int(0, 1);

            item.Drop(dropPosition, true);
        }

        items = new ItemStack[size];
    }

    public virtual void Open(Vector2Int holder)
    {
        this.holder = holder;
        open = true;
        UpdateMenuStatus();
    }

    public virtual void Close()
    {
        open = false;
        UpdateMenuStatus();
    }

    public virtual void UpdateMenuStatus()
    {
        ContainerInventoryMenu inventory = InventoryMenuManager.instance.containerInventoryMenu;
        inventory.active = open;
        inventory.inventory = this;
    }
}
