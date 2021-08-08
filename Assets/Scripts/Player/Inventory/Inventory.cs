using System;
using System.Collections.Generic;
using System.IO;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class Inventory : NetworkBehaviour
{
    public static int MaxStackSize = 64;
    [SyncVar] public Location holder;
    [SyncVar] public string invName;
    [SyncVar] public int size;
    [SyncVar] public string type;
    [SyncVar] public int id;
    [SyncVar] public GameObject inventoryMenu;
    [SyncVar] public bool open;

    public GameObject inventoryMenuPrefab;

    public SyncList<ItemStack> items = new SyncList<ItemStack>();

    private void Start()
    {
        WorldManager.instance.loadedInventories[id] = this;
    }

    public virtual void Update()
    {
        if ((Time.time % 5f) - Time.deltaTime <= 0 && isServer)
            Save();
    }

    [Server]
    public static Inventory Create(string type, int size, string invName)
    {
        int id = Random.Range(1, 9999999);

        return Create(type, size, invName, id);
    }

    [Server]
    public static Inventory Create(string type, int size, string invName, int id)
    {
        GameObject obj = Instantiate(Resources.Load<GameObject>("Prefabs/Inventories/" + type));
        Inventory inventory = obj.GetComponent<Inventory>();

        inventory.size = size;
        inventory.type = type;
        inventory.invName = invName;
        inventory.id = id;

        for (int i = 0; i < size; i++)
            inventory.items.Add(new ItemStack());

        WorldManager.instance.loadedInventories[id] = inventory;

        NetworkServer.Spawn(obj);

        return inventory;
    }

    public static Inventory Get(int id)
    {
        if (WorldManager.instance.loadedInventories.ContainsKey(id))
            return WorldManager.instance.loadedInventories[id];

        if (NetworkServer.active && Directory.Exists(WorldManager.world.getPath() + "\\inventories\\" + id))
            return Load(id);

        Debug.LogError("Failed getting inventory with id '" + id + "'");
        return null;
    }

    public static bool IsAnyOpen(PlayerInstance playerInstance)
    {
        foreach (Inventory inv in WorldManager.instance.loadedInventories.Values)
            if (inv.open && inv.inventoryMenu.GetComponent<InventoryMenu>().playerInstance == playerInstance.gameObject)
                return true;

        return false;
    }

    [Server]
    public void Save()
    {
        string path = WorldManager.world.getPath() + "\\inventories\\" + id;
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        if (!File.Exists(path + "\\items.dat"))
            File.Create(path + "\\items.dat").Close();
        if (!File.Exists(path + "\\type.dat"))
            File.Create(path + "\\type.dat").Close();
        if (!File.Exists(path + "\\invName.dat"))
            File.Create(path + "\\invName.dat").Close();
        if (!File.Exists(path + "\\size.dat"))
            File.Create(path + "\\size.dat").Close();

        List<string> itemLines = new List<string>();
        foreach (ItemStack item in items)
            itemLines.Add(item.GetSaveString());
        File.WriteAllLines(path + "\\items.dat", itemLines);

        File.WriteAllLines(path + "\\type.dat", new List<string> {type});

        File.WriteAllLines(path + "\\invName.dat", new List<string> {invName});

        File.WriteAllLines(path + "\\size.dat", new List<string> {size.ToString()});
    }

    [Server]
    public static Inventory Load(int id)
    {
        string path = WorldManager.world.getPath() + "\\inventories\\" + id;

        string[] itemLines = File.ReadAllLines(path + "\\items.dat");
        List<ItemStack> items = new List<ItemStack>();
        foreach (string itemLine in itemLines)
        {
            try
            {
                items.Add(new ItemStack(itemLine));
            }
            catch (Exception e)
            {
                Debug.LogError("Error in loading item to inventory, corrupted line: '" + itemLine + "'   " + e.Message);
            }
            finally
            {
                items.Add(new ItemStack());
            }
        }
        
        string type = File.ReadAllLines(path + "\\type.dat")[0];
        string invName = File.ReadAllLines(path + "\\invName.dat")[0];
        int size = int.Parse(File.ReadAllLines(path + "\\size.dat")[0]);

        Inventory inv = Create(type, size, invName, id);
        inv.items.Clear();
        inv.items.AddRange(items);

        return inv;
    }

    [Server]
    public void Delete()
    {
        string path = WorldManager.world.getPath() + "\\inventories\\" + id;
        Directory.Delete(path, true);
        NetworkServer.Destroy(gameObject);
    }

    [Server]
    public void SetItem(int slot, ItemStack item)
    {
        if (item.amount <= 0)
            item = new ItemStack();
        if (item.amount > MaxStackSize)
            item.amount = MaxStackSize;

        items[slot] = item;
    }

    public ItemStack GetItem(int slot)
    {
        if (slot < size)
            return items[slot];
        return null;
    }

    [Server]
    public void Clear()
    {
        for (int slot = 0; slot < size; slot++)
            items[slot] = new ItemStack();
    }

    [Server]
    public bool AddItem(ItemStack item)
    {
        for (int slot = 0; slot < size; slot++)
        {
            ItemStack invItem = GetItem(slot).Clone();

            if (invItem.material == item.material && invItem.amount + item.amount <= MaxStackSize)
            {
                invItem.amount += item.amount;
                SetItem(slot, invItem);
                return true;
            }
        }

        for (int slot = 0; slot < size; slot++)
            if (GetItem(slot).material == Material.Air)
            {
                SetItem(slot, item);
                return true;
            }

        return false;
    }

    [Server]
    public ItemStack GetItemOfMaterial(Material mat)
    {
        foreach (ItemStack item in items)
            if (item.material == mat)
                return item;
        return null;
    }

    public bool ContainsAtLeast(Material mat, int amount)
    {
        int amountOfMaterial = 0;
        foreach (ItemStack item in items)
        {
            if (item.material == mat)
                amountOfMaterial += item.amount;

            if (amountOfMaterial >= amount)
                return true;
        }

        return false;
    }

    public bool Contains(Material mat)
    {
        foreach (ItemStack item in items)
            if (item.material == mat)
                return true;
        return false;
    }

    [Server]
    public void DropAll(Location dropPosition)
    {
        foreach (ItemStack item in items)
            item.Drop(dropPosition + new Location(0, 1), true);

        Clear();
    }

    [Server]
    public void Open(PlayerInstance playerInstance)
    {
        if (open)
        {
            ChatManager.instance.AddMessagePlayer("Two players can't open the same inventory", playerInstance);
            return;
        }
        
        GameObject obj = Instantiate(inventoryMenuPrefab);
        InventoryMenu menu = obj.GetComponent<InventoryMenu>();

        menu.inventoryIds.Add(0, id);
        menu.playerInstance = playerInstance.gameObject;

        NetworkServer.Spawn(obj);
        inventoryMenu = obj;
        open = true;
    }

    [Server]
    public virtual void Close()
    {
        if (inventoryMenu != null)
            inventoryMenu.GetComponent<InventoryMenu>().Close();
        open = false;
    }

    [Command(requiresAuthority = false)]
    public void RequestClose()
    {
        Close();
    }
}