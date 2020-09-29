using UnityEngine;
using UnityEngine.UI;

public class InventoryMenu : MonoBehaviour
{
    public static PlayerInventory playerInventory;

    public bool active;

    private int inventoryAge;
    public Text playerInventoryTitle;
    public PointerSlot pointerSlot;

    public Transform slotList;

    public virtual bool wholePlayerInventory { get; } = false;

    public virtual void Update()
    {
        if (Player.localInstance != null)
            playerInventory = Player.localInstance.inventory;

        if (playerInventory == null)
            return;

        if (active)
            inventoryAge++;
        else inventoryAge = 0;

        CheckClose();

        GetComponent<CanvasGroup>().alpha = active ? 1 : 0;
        GetComponent<CanvasGroup>().interactable = active;
        GetComponent<CanvasGroup>().blocksRaycasts = active;

        playerInventory = Player.localInstance.inventory;

        SetTitle();

        FillSlots();
    }

    public virtual void CheckClose()
    {
        if (active && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E)) && inventoryAge > 10)
            Close();
    }

    public virtual void Close()
    {
        playerInventory.Close();
    }

    public virtual void SetTitle()
    {
        playerInventoryTitle.text = playerInventory.name;
    }

    public virtual void FillSlots()
    {
        if (active)
        {
            var size = wholePlayerInventory ? playerInventory.size : playerInventory.baseInventorySize;

            for (var i = 0; i < size; i++) getSlotObject(i).item = getItem(i);
        }
    }

    public virtual ItemSlot getSlotObject(int index)
    {
        if (index < slotList.childCount)
            return getSlotObjects()[index];
        return null;
    }

    public ItemSlot[] getSlotObjects()
    {
        return slotList.GetComponentsInChildren<ItemSlot>();
    }

    public virtual ItemStack getItem(int index)
    {
        return playerInventory.getItem(index);
    }

    public virtual void OnClickSlot(int slotIndex, int clickType)
    {
        if (clickType == 0)
            OnRightClickSlot(slotIndex);
        else
            OnLeftClickSlot(slotIndex);

        pointerSlot.UpdateSlot();
    }

    public virtual void OnLeftClickSlot(int slotIndex)
    {
        var slotItem = getItem(slotIndex);
        var pointerItem = pointerSlot.item;

        if ((slotItem.material == Material.Air || slotItem.material == pointerItem.material)
            && pointerItem.amount > 0) //Left Click to leave one item
        {
            SlotAction_LeaveOne(slotIndex);

            return;
        }

        if ((pointerItem.amount == 0 || pointerItem.material == Material.Air) &&
            slotItem.amount > 0) //Left click to halve
            SlotAction_Halve(slotIndex);
    }

    public virtual void OnRightClickSlot(int slotIndex)
    {
        var slotItem = getItem(slotIndex);
        var pointerItem = pointerSlot.item;

        //Right click to swap
        if (pointerItem.material == Material.Air || pointerItem.material != slotItem.material)
            SlotAction_Swap(slotIndex);
        else
            //Right click to add pointer to slot
            SlotAction_MergeSlot(slotIndex);
    }

    public virtual void SlotAction_LeaveOne(int slotIndex)
    {
        var slotItem = getItem(slotIndex);
        var pointerItem = pointerSlot.item;

        slotItem.material = pointerItem.material;
        slotItem.amount++;
        pointerItem.amount--;
    }

    public virtual void SlotAction_Halve(int slotIndex)
    {
        var slotItem = getItem(slotIndex);
        var pointerItem = pointerSlot.item;

        pointerItem.material = slotItem.material;

        var pointerAmount = Mathf.CeilToInt(slotItem.amount / 2f);
        var slotAmount = Mathf.FloorToInt(slotItem.amount / 2f);

        pointerItem.amount = pointerAmount;
        slotItem.amount = slotAmount;
    }

    public virtual void SlotAction_Swap(int slotIndex)
    {
        var slotItem = getItem(slotIndex);
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

    public virtual void SlotAction_MergeSlot(int slotIndex)
    {
        var slotItem = getItem(slotIndex);
        var pointerItem = pointerSlot.item;

        slotItem.amount += pointerItem.amount;
        pointerItem.amount = 0;
    }
}