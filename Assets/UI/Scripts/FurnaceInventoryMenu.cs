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
            yield return new WaitForSeconds(1f);

            if (active)
                UpdateInventory();
        }
    }

    public override void UpdateInventory()
    {
        base.UpdateInventory();

        if (!(inventories[1] is FurnaceInventory))
            return;

        fuelProgress.fillAmount = ((FurnaceInventory) inventories[1]).fuelLeft / ((FurnaceInventory) inventories[1]).highestFuel;
        if (((FurnaceInventory) inventories[1]).highestFuel == 0)
            fuelProgress.fillAmount = 0;
        smeltingProgress.fillAmount = ((FurnaceInventory) inventories[1]).smeltingProgress / SmeltingRecepie.smeltTime;
    }

    public override void OnClickSlot(int inventory, int slotIndex, int clickType)
    {
        if (inventory == 1 && slotIndex == ((FurnaceInventory) inventories[1]).getResultSlot())
        {
            OnClickSmeltingResultSlot(slotIndex, clickType);
            base.OnClickSlot(inventory,slotIndex, -1);
            return;
        }

        base.OnClickSlot(inventory, slotIndex, clickType);
    }

    public virtual void OnClickSmeltingResultSlot(int slotIndex, int clickType)
    {
        if (inventories[1].getItem(((FurnaceInventory) inventories[1]).getResultSlot()).material == Material.Air) 
            return;
        if (inventories[1].getItem(((FurnaceInventory) inventories[1]).getResultSlot()).material != pointerSlot.item.material && 
            pointerSlot.item.material != Material.Air) 
            return;

        pointerSlot.item.material = inventories[1].getItem(((FurnaceInventory) inventories[1]).getResultSlot()).material;
        pointerSlot.item.amount += inventories[1].getItem(((FurnaceInventory) inventories[1]).getResultSlot()).amount;
        inventories[1].setItem(((FurnaceInventory) inventories[1]).getResultSlot(), new ItemStack());
    }
}