using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryMenu : MonoBehaviour
{
    public PlayerInventory playerInventory;

    public Transform slotList;
    public PointerSlot pointerSlot;

    public static bool active = false;

    public virtual int totalSlotAmount { get; set; } = 45;
        
    void Update()
    {
        if (Player.localInstance != null)
            playerInventory = Player.localInstance.inventory;


        if (playerInventory == null)
            return;

        GetComponent<CanvasGroup>().alpha = (active) ? 1 : 0;
        GetComponent<CanvasGroup>().interactable = (active);
        GetComponent<CanvasGroup>().blocksRaycasts = (active);

        playerInventory = Player.localInstance.inventory;

        FillSlots();
    }

    public virtual void FillSlots()
    {
        if (active)
        {
            for (int i = 0; i < totalSlotAmount; i++)
            {
                getSlot(i).item = playerInventory.getItem(i);
            }
        }
    }

    public ItemSlot getSlot(int index)
    {
        return slotList.GetChild(index).GetComponent<ItemSlot>();
    }

    public virtual void OnClickSlot(int slotIndex, int clickType)
    {
        if (clickType == 0)
        {
            OnRightClickSlot(slotIndex);
        }
        else
        {
            OnLeftClickSlot(slotIndex);
        }
    }

    public virtual void OnLeftClickSlot(int slotIndex)
    {
        ItemStack slotItem = playerInventory.getItem(slotIndex).Clone();
        ItemStack pointerItem = pointerSlot.item.Clone();

        if ((slotItem.material == Material.Air || slotItem.material == pointerItem.material)
            && pointerItem.amount > 0)      //Left Click to leave one item
        {
            SlotAction_LeaveOne(slotIndex);

            return;
        }
        if ((pointerItem.amount == 0 || pointerItem.material == Material.Air) &&
            (slotItem.amount > 0))      //Left click to halve
        {
            SlotAction_Halve(slotIndex);

            return;
        }
    }

    public virtual void OnRightClickSlot(int slotIndex)
    {
        ItemStack slotItem = playerInventory.getItem(slotIndex).Clone();
        ItemStack pointerItem = pointerSlot.item.Clone();

        //Right click to swap
        if (pointerItem.material == Material.Air || pointerItem.material != slotItem.material)
        {
            SlotAction_Swap(slotIndex);
            return;
        }
        else
        {       //Right click to add pointer to slot
            SlotAction_MergeSlot(slotIndex);

            return;
        }
    }

    public virtual void SlotAction_LeaveOne(int slotIndex)
    {
        ItemStack slotItem = playerInventory.getItem(slotIndex).Clone();
        ItemStack pointerItem = pointerSlot.item.Clone();

        slotItem.material = pointerItem.material;
        slotItem.amount++;
        pointerItem.amount--;

        playerInventory.setItem(slotIndex, slotItem);
        pointerSlot.item = pointerItem;
    }

    public virtual void SlotAction_Halve(int slotIndex)
    {
        ItemStack slotItem = playerInventory.getItem(slotIndex).Clone();
        ItemStack pointerItem = pointerSlot.item.Clone();

        pointerItem.material = slotItem.material;

        int pointerAmount = Mathf.CeilToInt((float)slotItem.amount / 2f);
        int slotAmount = Mathf.FloorToInt((float)slotItem.amount / 2f);

        pointerItem.amount = pointerAmount;
        slotItem.amount = slotAmount;

        playerInventory.setItem(slotIndex, slotItem);
        pointerSlot.item = pointerItem;
    }

    public virtual void SlotAction_Swap(int slotIndex)
    {
        ItemStack slotItem = playerInventory.getItem(slotIndex).Clone();
        ItemStack pointerItem = pointerSlot.item.Clone();

        playerInventory.setItem(slotIndex, pointerItem);
        pointerSlot.item = slotItem;
    }

    public virtual void SlotAction_MergeSlot(int slotIndex)
    {
        ItemStack slotItem = playerInventory.getItem(slotIndex).Clone();
        ItemStack pointerItem = pointerSlot.item.Clone();

        slotItem.amount += pointerItem.amount;
        pointerItem.amount = 0;

        playerInventory.setItem(slotIndex, slotItem);
        pointerSlot.item = pointerItem;

    }
}