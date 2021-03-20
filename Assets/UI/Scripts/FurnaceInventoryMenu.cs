using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using Mirror;

public class FurnaceInventoryMenu : ContainerInventoryMenu
{
    
    public Image fuelProgress;
    public Image smeltingProgress;

    public override void OnStartServer()
    {
        StartCoroutine(furnaceInvenoryLoop());
    }

    IEnumerator furnaceInvenoryLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            UpdateInventory();
        }
    }

    public override void UpdateInventory()
    {
        base.UpdateInventory();
        
        FurnaceInventory inv = ((FurnaceInventory) Inventory.Get(inventoryIds[0]));

        fuelProgress.fillAmount = inv.fuelLeft / inv.highestFuel;
        if (inv.highestFuel == 0)
            fuelProgress.fillAmount = 0;
        smeltingProgress.fillAmount = inv.smeltingProgress / SmeltingRecipe.smeltTime;
    }

    [Client]
    public override void OnClickSlot(int inventoryIndex, int slotIndex, int clickType)
    {
        FurnaceInventory inv = ((FurnaceInventory) Inventory.Get(inventoryIds[0]));
        
        if (inventoryIndex == 1 && slotIndex == inv.GetResultSlot())
        {
            OnClickSmeltingResultSlot();
            return;
        }

        base.OnClickSlot(inventoryIndex, slotIndex, clickType);
    }

    [Command(requiresAuthority = false)]
    public virtual void OnClickSmeltingResultSlot()
    {
        FurnaceInventory inv = ((FurnaceInventory) Inventory.Get(inventoryIds[0]));
        
        if (inv.GetItem(inv.GetResultSlot()).material == Material.Air) 
            return;
        if (inv.GetItem(inv.GetResultSlot()).material != pointerItem.material && 
            pointerSlot.item.material != Material.Air) 
            return;

        pointerItem.material = inv.GetItem(inv.GetResultSlot()).material;
        pointerItem.amount += inv.GetItem(inv.GetResultSlot()).amount;
        inv.SetItem(inv.GetResultSlot(), new ItemStack());
        
        UpdateInventory();
    }
}