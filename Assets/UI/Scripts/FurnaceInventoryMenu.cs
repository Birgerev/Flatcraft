using UnityEngine.UI;
using UnityEngine;
using System.Collections;

public class FurnaceInventoryMenu : ContainerInventoryMenu
{
    public Image fuelProgress;
    public Image smeltingProgress;

    private void Start()
    {
        StartCoroutine(furnaceInvenoryLoop());
    }

    IEnumerator furnaceInvenoryLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);

            if (active)
                UpdateInventory();
        }
    }

    public override void UpdateInventory()
    {
        base.UpdateInventory();

        if (inventory == null || inventory as FurnaceInventory == null)
            return;

        fuelProgress.fillAmount = ((FurnaceInventory) inventory).fuelLeft / ((FurnaceInventory) inventory).highestFuel;
        if (((FurnaceInventory) inventory).highestFuel == 0)
            fuelProgress.fillAmount = 0;
        smeltingProgress.fillAmount = ((FurnaceInventory) inventory).smeltingProgress / SmeltingRecepie.smeltTime;
    }

    public override void OnClickSlot(int slotIndex, int clickType)
    {
        if (slotIndex == playerInventory.baseInventorySize + ((FurnaceInventory) inventory).getResultSlot())
        {
            OnClickSmeltingResultSlot(slotIndex, clickType);
            base.OnClickSlot(-1, -1);
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