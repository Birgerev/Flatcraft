using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Linq;

public class InventoryMenu : MonoBehaviour
{
    public static Dictionary<int, Inventory> inventories = new Dictionary<int, Inventory>();
    public List<Transform> inventorySlotLists = new List<Transform>();

    public bool active;

    private float inventoryAge;
    public Text playerInventoryTitle;
    public PointerSlot pointerSlot;

    public virtual void Update()
    {
        //Assign player inventory
        if (Player.localInstance != null)
            inventories[0] = Player.localInstance.inventory;

        if (active)
        {
            SetTitle();
            CheckClose();
            
            if (inventoryAge == 0)
                ScheduleUpdateInventory();
            
            inventoryAge += Time.deltaTime;
        }
        else inventoryAge = 0;

        GetComponent<CanvasGroup>().alpha = active ? 1 : 0;
        GetComponent<CanvasGroup>().interactable = active;
        GetComponent<CanvasGroup>().blocksRaycasts = active;
    }

    public virtual void CheckClose()
    {
        if (active && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E)) && inventoryAge > 0.1f)
            Close();
    }

    public void Close()
    {
        //Close inventories
        foreach(Inventory inventory in inventories.Values.ToList())
            inventory.Close();
    }

    public virtual void SetTitle()
    {
        playerInventoryTitle.text = inventories[0].name;
    }

    public virtual void ScheduleUpdateInventory()
    {
        StartCoroutine(scheduleUpdateInventory());
    }

    IEnumerator scheduleUpdateInventory()
    {
        yield return new WaitForSeconds(0.01f);

        if(active)
            UpdateInventory();
    }

    public virtual void UpdateInventory()
    {
        UpdateSlots();
    }
    
    public void UpdateSlots()
    {
        //Update every slot in every inventory
        for (int inventoryId = 0; inventoryId < inventorySlotLists.Count; inventoryId++)
        {
            ItemSlot[] slots = GetSlots(inventoryId);

            for (int slotId = 0; slotId < slots.Length; slotId++)
            {
                slots[slotId].item = GetItem(inventoryId, slotId);
                slots[slotId].UpdateSlot();
            }
        }

        //Update the pointer slot aswell
        pointerSlot.UpdateSlot();
    }

    public ItemSlot GetSlot(int inventory, int index)
    {
        if (index < GetSlots(inventory).Length)
            return GetSlots(inventory)[index];
        return null;
    }

    public ItemSlot[] GetSlots(int inventory)
    {
        return inventorySlotLists[inventory].GetComponentsInChildren<ItemSlot>();
    }

    public ItemStack GetItem(int inventory, int index)
    {
        return inventories[inventory].getItem(index);
    }
    
    public int GetSlotIndex(ItemSlot slot)
    {
        return slot.transform.GetSiblingIndex();
    }

    public int GetSlotInventoryId(ItemSlot slot)
    {
        for (int inventoryid = 0; inventoryid < inventorySlotLists.Count; inventoryid++)
        {
            Transform inventorySlotList = inventorySlotLists[inventoryid];

            if (slot.transform.IsChildOf(inventorySlotList))
                return inventoryid;
        }
        
        Debug.LogError("No tracked inventory was found for slot '" + slot.gameObject.name+"' in " + this.name);
        return -1;
    }

    public virtual void OnClickSlot(int inventory, int slotIndex, int clickType)
    {
        if (clickType == 0)
            OnLeftClickSlot(inventory, slotIndex);
        else if (clickType == 1)
            OnRightClickSlot(inventory, slotIndex);

        ScheduleUpdateInventory();
    }

    public virtual void OnRightClickSlot(int inventory, int slotIndex)
    {
        var slotItem = GetItem(inventory, slotIndex);
        var pointerItem = pointerSlot.item;

        if ((slotItem.material == Material.Air || slotItem.material == pointerItem.material)
            && pointerItem.amount > 0 && slotItem.amount + 1 <= 64) //Right Click to leave one item
        {
            SlotAction_LeaveOne(inventory, slotIndex);

            return;
        }

        if ((pointerItem.amount == 0 || pointerItem.material == Material.Air) &&
            slotItem.amount > 0) //Right click to halve
            SlotAction_Halve(inventory, slotIndex);
    }

    public virtual void OnLeftClickSlot(int inventory, int slotIndex)
    {
        var slotItem = GetItem(inventory, slotIndex);
        var pointerItem = pointerSlot.item;

        //Left click to swap
        if (pointerItem.material == Material.Air || pointerItem.material != slotItem.material)
            SlotAction_Swap(inventory, slotIndex);
        else
            //Left click to add pointer to slot
            SlotAction_MergeSlot(inventory, slotIndex);
    }

    public virtual void SlotAction_LeaveOne(int inventory, int slotIndex)
    {
        var slotItem = GetItem(inventory, slotIndex);
        var pointerItem = pointerSlot.item;

        slotItem.material = pointerItem.material;
        slotItem.amount++;
        pointerItem.amount--;
    }

    public virtual void SlotAction_Halve(int inventory, int slotIndex)
    {
        var slotItem = GetItem(inventory, slotIndex);
        var pointerItem = pointerSlot.item;

        pointerItem.material = slotItem.material;

        var pointerAmount = Mathf.CeilToInt(slotItem.amount / 2f);
        var slotAmount = Mathf.FloorToInt(slotItem.amount / 2f);

        pointerItem.amount = pointerAmount;
        slotItem.amount = slotAmount;
    }

    public virtual void SlotAction_Swap(int inventory, int slotIndex)
    {
        var slotItem = GetItem(inventory, slotIndex);
        var pointerItem = pointerSlot.item;

        var slotItemClone = slotItem.Clone();
        var pointerItemClone = pointerItem.Clone();


        slotItem.material = pointerItemClone.material;
        slotItem.amount = pointerItemClone.amount;
        slotItem.data = pointerItemClone.data;
        slotItem.durability = pointerItemClone.durability;

        pointerItem.material = slotItemClone.material;
        pointerItem.amount = slotItemClone.amount;
        pointerItem.data = slotItemClone.data;
        pointerItem.durability = slotItemClone.durability;
    }

    public virtual void SlotAction_MergeSlot(int inventory, int slotIndex)
    {
        var slotItem = GetItem(inventory, slotIndex);
        var pointerItem = pointerSlot.item;

        int maxAmountOfItemsToMerge = Inventory.MaxStackSize - slotItem.amount;
        int amountOfItemsFromPointerToMerge = Mathf.Clamp(pointerItem.amount, 0, maxAmountOfItemsToMerge);

        slotItem.amount += amountOfItemsFromPointerToMerge;
        pointerItem.amount -= amountOfItemsFromPointerToMerge;
    }
}