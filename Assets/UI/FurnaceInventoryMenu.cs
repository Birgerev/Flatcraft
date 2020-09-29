using UnityEngine.UI;

public class FurnaceInventoryMenu : ContainerInventoryMenu
{
    public Image fuelProgress;
    public Image smeltingProgress;

    public override void FillSlots()
    {
        base.FillSlots();

        if (inventory == null || inventory as FurnaceInventory == null)
            return;

        fuelProgress.fillAmount = ((FurnaceInventory) inventory).fuelLeft / ((FurnaceInventory) inventory).highestFuel;
        if (((FurnaceInventory) inventory).highestFuel == 0)
            fuelProgress.fillAmount = 0;
        smeltingProgress.fillAmount = ((FurnaceInventory) inventory).smeltingProgress / SmeltingRecepie.smeltTime;
        //TODO show fuel left
    }

    public override void OnClickSlot(int slotIndex, int clickType)
    {
        if (slotIndex == playerInventory.baseInventorySize + ((FurnaceInventory) inventory).getResultSlot())
        {
            OnClickSmeltingResultSlot(slotIndex, clickType);
            return;
        }

        base.OnClickSlot(slotIndex, clickType);
    }

    public virtual void OnClickSmeltingResultSlot(int slotIndex, int clickType)
    {
        if (((FurnaceInventory) inventory).getItem(((FurnaceInventory) inventory).getResultSlot()).material ==
            Material.Air) return;
        if (((FurnaceInventory) inventory).getItem(((FurnaceInventory) inventory).getResultSlot()).material !=
            pointerSlot.item.material && pointerSlot.item.material != Material.Air) return;

        pointerSlot.item.material = ((FurnaceInventory) inventory)
            .getItem(((FurnaceInventory) inventory).getResultSlot()).material;
        pointerSlot.item.amount += ((FurnaceInventory) inventory)
            .getItem(((FurnaceInventory) inventory).getResultSlot()).amount;
        ((FurnaceInventory) inventory).setItem(((FurnaceInventory) inventory).getResultSlot(), new ItemStack());
    }
}