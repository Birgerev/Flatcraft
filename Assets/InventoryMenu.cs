using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class InventoryMenu : MonoBehaviour
{
    public static PlayerInventory playerInventory;

    public Transform slotList;
    public Text playerInventoryTitle;
    public PointerSlot pointerSlot;

    public bool active = false;


    public virtual void Update()
    {
        if (Player.localInstance != null)
            playerInventory = Player.localInstance.inventory;
        
        if (playerInventory == null)
            return;

        if (active && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E)))
            active = false;

        GetComponent<CanvasGroup>().alpha = (active) ? 1 : 0;
        GetComponent<CanvasGroup>().interactable = (active);
        GetComponent<CanvasGroup>().blocksRaycasts = (active);

        playerInventory = Player.localInstance.inventory;

        SetTitle();

        FillSlots();
    }

    public virtual void SetTitle()
    {
        playerInventoryTitle.text = playerInventory.name;
    }

    public virtual void FillSlots()
    {
        if (active)
        {
            for (int i = 0; i < playerInventory.size; i++)
            {
                getSlotObject(i).item = getItem(i);
            }
        }
    }

    public virtual ItemSlot getSlotObject(int index)
    {
        return slotList.GetChild(index).GetComponent<ItemSlot>();
    }


    public virtual ItemStack getItem(int index)
    {
        return playerInventory.getItem(index);
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
        ItemStack slotItem = getItem(slotIndex);
        ItemStack pointerItem = pointerSlot.item;

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
        ItemStack slotItem = getItem(slotIndex);
        ItemStack pointerItem = pointerSlot.item;
        
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
        ItemStack slotItem = getItem(slotIndex);
        ItemStack pointerItem = pointerSlot.item;

        slotItem.material = pointerItem.material;
        slotItem.amount++;
        pointerItem.amount--;
    }

    public virtual void SlotAction_Halve(int slotIndex)
    {
        ItemStack slotItem = getItem(slotIndex);
        ItemStack pointerItem = pointerSlot.item;

        pointerItem.material = slotItem.material;

        int pointerAmount = Mathf.CeilToInt((float)slotItem.amount / 2f);
        int slotAmount = Mathf.FloorToInt((float)slotItem.amount / 2f);

        pointerItem.amount = pointerAmount;
        slotItem.amount = slotAmount;
    }

    public virtual void SlotAction_Swap(int slotIndex)
    {
        ItemStack slotItem = getItem(slotIndex);
        ItemStack pointerItem = pointerSlot.item;

        ItemStack slotItemClone = slotItem.Clone();
        ItemStack pointerItemClone = pointerItem.Clone();



        slotItem.material = pointerItemClone.material;
        slotItem.amount = pointerItemClone.amount;
        slotItem.data = pointerItemClone.data;

        pointerItem.material = slotItemClone.material;
        pointerItem.amount = slotItemClone.amount;
        pointerItem.data = slotItemClone.data;
    }

    public virtual void SlotAction_MergeSlot(int slotIndex)
    {
        ItemStack slotItem = getItem(slotIndex);
        ItemStack pointerItem = pointerSlot.item;

        slotItem.amount += pointerItem.amount;
        pointerItem.amount = 0;
    }
}