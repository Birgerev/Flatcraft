using UnityEngine.UI;

public class ContainerInventoryMenu : InventoryMenu
{
    public Inventory inventory;
    public Text inventoryTitle;


    public override void SetTitle()
    {
        base.SetTitle();
        inventoryTitle.text = inventory.name;
    }

    public override ItemStack getItem(int index)
    {
        if (index < playerInventory.baseInventorySize)
            return base.getItem(index);
        if (index < playerInventory.baseInventorySize + inventory.size)
            return inventory.getItem(index - playerInventory.baseInventorySize);

        return new ItemStack();
    }

    public override void Close()
    {
        base.Close();
        inventory.Close();
    }

    public override void FillSlots()
    {
        base.FillSlots();
        for (var i = playerInventory.baseInventorySize; i < playerInventory.baseInventorySize + inventory.size; i++)
            if (getSlotObject(i) != null)
                getSlotObject(i).item = getItem(i);
    }
    public override void UpdateSlots()
    {
        base.UpdateSlots();

        for (var i = playerInventory.baseInventorySize; i < playerInventory.baseInventorySize + inventory.size; i++)
            if (getSlotObject(i) != null)
                getSlotObject(i).UpdateSlot();
    }
}