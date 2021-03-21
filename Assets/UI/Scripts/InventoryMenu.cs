using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Linq;
using Mirror;

public class InventoryMenu : NetworkBehaviour
{
    public SyncDictionary<int, int> inventoryIds = new SyncDictionary<int, int>();
    [SyncVar] public ItemStack pointerItem;
    [SyncVar] public GameObject playerInstance;

    public List<Transform> inventorySlotLists = new List<Transform>();
    public PointerSlot pointerSlot;
    public CanvasGroup canvasGroup;
    public Text inventoryTitle;
    private float inventoryAge;

    public override void OnStartClient()
    {
        ClientInventoryUpdate();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        OpenPlayerInventory();
    }

    public virtual void Update()
    {
        bool ownsInventoryMenu = (PlayerInstance.localPlayerInstance.gameObject == playerInstance);
        canvasGroup.alpha = ownsInventoryMenu ? 1 : 0;
        canvasGroup.interactable = ownsInventoryMenu;
        canvasGroup.blocksRaycasts = ownsInventoryMenu;
        if (ownsInventoryMenu)
        {
            CheckClose();
        
            inventoryAge += Time.deltaTime;
        }
    }

    public virtual void OpenPlayerInventory()
    {
        inventoryIds.Add(1,
            playerInstance.GetComponent<PlayerInstance>().playerEntity.GetComponent<Player>().inventoryId);
    }
    
    [Command(requiresAuthority = false)]
    public virtual void RequestInventoryUpdate()
    {
        UpdateInventory();
    }
    
    [Server]
    public virtual void UpdateInventory()
    {
        CallClientInventoryUpdate();
    }

    [ClientRpc]
    public virtual void CallClientInventoryUpdate()
    {
        StartCoroutine(awaitInventoryChangesToUpdate());
    }

    IEnumerator awaitInventoryChangesToUpdate()
    {
        yield return new WaitForSeconds(0f);
        ClientInventoryUpdate();
    }
    
    [Client]
    public virtual void ClientInventoryUpdate()
    {
        SetTitle();
        UpdateSlots();
    }

    [Client]
    public virtual void CheckClose()
    {
        if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E)) && inventoryAge > 0.1f)
            foreach(int inventoryId in inventoryIds.Values.ToList())
                Inventory.Get(inventoryId).RequestClose();
    }

    [Server]
    public void Close()
    {
        NetworkServer.Destroy(gameObject);
    }

    [Client]
    public virtual void SetTitle()
    {
        inventoryTitle.text = Inventory.Get(inventoryIds[0]).invName;
    }
    
    [Client]
    public void UpdateSlots()
    {
        //Update every slot in every inventory
        for (int inventoryIndex = 0; inventoryIndex < inventorySlotLists.Count; inventoryIndex++)
        {
            ItemSlot[] slots = GetSlots(inventoryIndex);

            for (int slotId = 0; slotId < slots.Length; slotId++)
            {
                slots[slotId].item = GetItem(inventoryIndex, slotId);
                slots[slotId].UpdateSlot();
            }
        }

        //Update the pointer slot aswell
        pointerSlot.item = pointerItem;
        pointerSlot.UpdateSlot();
    }

    [Client]
    public ItemSlot GetSlot(int inventoryIndex, int slot)
    {
        if (slot < GetSlots(inventoryIndex).Length)
            return GetSlots(inventoryIndex)[slot];
        return null;
    }

    [Client]
    public ItemSlot[] GetSlots(int inventoryIndex)
    {
        return inventorySlotLists[inventoryIndex].GetComponentsInChildren<ItemSlot>();
    }

    public ItemStack GetItem(int inventoryIndex, int slot)
    {
        return Inventory.Get(inventoryIds[inventoryIndex]).GetItem(slot);
    }

    [Server]
    public void SetItem(int inventoryIndex, int slot, ItemStack item)
    {
        Inventory.Get(inventoryIds[inventoryIndex]).SetItem(slot, item);
    }
    
    [Server]
    public void SetPointerItem(ItemStack item)
    {
        pointerItem = item;
    }
    
    [Client]
    public int GetSlotIndex(ItemSlot slot)
    {
        return slot.transform.GetSiblingIndex();
    }

    [Client]
    public int GetSlotInventoryIndex(ItemSlot slot)
    {
        for (int inventoryIndex = 0; inventoryIndex < inventorySlotLists.Count; inventoryIndex++)
        {
            Transform inventorySlotList = inventorySlotLists[inventoryIndex];

            if (slot.transform.IsChildOf(inventorySlotList))
                return inventoryIndex;
        }
        
        Debug.LogError("No tracked inventory was found for slot '" + slot.gameObject.name+"' in " + this.name);
        return -1;
    }

    [Client]
    public virtual void OnClickSlot(int inventoryIndex, int slotIndex, int clickType)
    {
        if (clickType == 0)
            OnLeftClickSlot(inventoryIndex, slotIndex);
        else if (clickType == 1)
            OnRightClickSlot(inventoryIndex, slotIndex);
    }

    [Command(requiresAuthority = false)]
    public virtual void OnRightClickSlot(int inventoryIndex, int slotIndex)
    {
        ItemStack slotItem = GetItem(inventoryIndex, slotIndex);
        
        //Right Click to leave one item
        if ((slotItem.material == Material.Air || slotItem.material == pointerItem.material)
            && pointerItem.amount > 0 && slotItem.amount + 1 <= 64) 
        {
            SlotAction_LeaveOne(inventoryIndex, slotIndex);

            return;
        }
        
        //Right click to halve
        if ((pointerItem.amount == 0 || pointerItem.material == Material.Air) && slotItem.amount > 0)
            SlotAction_Halve(inventoryIndex, slotIndex);
    }

    [Command(requiresAuthority = false)]
    public virtual void OnLeftClickSlot(int inventoryIndex, int slotIndex)
    {
        var slotItem = GetItem(inventoryIndex, slotIndex);

        //Left click to swap
        if (pointerItem.material == Material.Air || pointerItem.material != slotItem.material)
            SlotAction_Swap(inventoryIndex, slotIndex);
        else
            //Left click to add pointer to slot
            SlotAction_MergeSlot(inventoryIndex, slotIndex);
    }

    [Server]
    public virtual void SlotAction_LeaveOne(int inventoryIndex, int slotIndex)
    {
        ItemStack slotItem = GetItem(inventoryIndex, slotIndex).Clone();
        ItemStack pointerItemClone = pointerItem.Clone();

        slotItem.material = pointerItem.material;
        slotItem.amount++;
        SetItem(inventoryIndex, slotIndex, slotItem);
        
        pointerItemClone.amount--;
        SetPointerItem(pointerItemClone);
        
        UpdateInventory();
    }

    [Server]
    public virtual void SlotAction_Halve(int inventoryIndex, int slotIndex)
    {
        ItemStack slotItem = GetItem(inventoryIndex, slotIndex).Clone();
        ItemStack pointerItemClone = pointerItem.Clone();

        int pointerAmount = Mathf.CeilToInt(slotItem.amount / 2f);
        int slotAmount = Mathf.FloorToInt(slotItem.amount / 2f);

        pointerItemClone.material = slotItem.material;
        pointerItemClone.amount = pointerAmount;
        SetPointerItem(pointerItemClone);
        
        slotItem.amount = slotAmount;
        SetItem(inventoryIndex, slotIndex, slotItem);
        
        UpdateInventory();
    }

    [Server]
    public virtual void SlotAction_Swap(int inventoryIndex, int slotIndex)
    {
        ItemStack slotItem = GetItem(inventoryIndex, slotIndex);
        ItemStack pointerItemClone = pointerItem.Clone();

        SetItem(inventoryIndex, slotIndex, pointerItemClone);
        SetPointerItem(slotItem);
        
        UpdateInventory();
    }

    [Server]
    public virtual void SlotAction_MergeSlot(int inventoryIndex, int slotIndex)
    {
        ItemStack slotItem = GetItem(inventoryIndex, slotIndex).Clone();
        ItemStack pointerItemClone = pointerItem.Clone();

        int maxAmountOfItemsToMerge = Inventory.MaxStackSize - slotItem.amount;
        int amountOfItemsFromPointerToMerge = Mathf.Clamp(pointerItemClone.amount, 0, maxAmountOfItemsToMerge);

        pointerItemClone.amount -= amountOfItemsFromPointerToMerge;
        slotItem.amount += amountOfItemsFromPointerToMerge;
        SetItem(inventoryIndex, slotIndex, slotItem);
        SetPointerItem(pointerItemClone);
        
        UpdateInventory();
    }
}