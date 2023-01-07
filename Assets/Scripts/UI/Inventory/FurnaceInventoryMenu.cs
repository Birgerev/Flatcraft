using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class FurnaceInventoryMenu : ContainerInventoryMenu
{
    public Image fuelProgress;
    public Image smeltingProgress;

    public override void OnStartServer()
    {
        base.OnStartServer();

        StartCoroutine(furnaceInvenoryLoop());
    }

    private IEnumerator furnaceInvenoryLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            UpdateInventory();
        }
    }

    [Client]
    public override void ClientInventoryUpdate()
    {
        base.ClientInventoryUpdate();

        FurnaceInventory inv = (FurnaceInventory) Inventory.Get(inventoryIds[0]);

        fuelProgress.fillAmount = inv.fuelLeft / inv.highestFuel;
        if (inv.highestFuel == 0)
            fuelProgress.fillAmount = 0;
        smeltingProgress.fillAmount = inv.smeltingProgress / SmeltingRecipe.smeltTime;
    }

    [Client]
    public override void OnClickSlot(int inventoryIndex, int slotIndex, ClickType clickType)
    {
        FurnaceInventory inv = (FurnaceInventory) Inventory.Get(inventoryIds[0]);

        if (inventoryIndex == 0 && slotIndex == inv.GetResultSlot())
        {
            OnClickSmeltingResultSlot();
            return;
        }

        base.OnClickSlot(inventoryIndex, slotIndex, clickType);
    }

    [Command(requiresAuthority = false)]
    public virtual void OnClickSmeltingResultSlot()
    {
        FurnaceInventory inv = (FurnaceInventory) Inventory.Get(inventoryIds[0]);
        ItemStack newPointerItem = pointerItem;

        if (inv.GetItem(inv.GetResultSlot()).material == Material.Air)
            return;
        if (inv.GetItem(inv.GetResultSlot()).material != newPointerItem.material &&
            pointerSlot.item.material != Material.Air)
            return;

        newPointerItem.material = inv.GetItem(inv.GetResultSlot()).material;
        newPointerItem.Amount += inv.GetItem(inv.GetResultSlot()).Amount;
        inv.SetItem(inv.GetResultSlot(), new ItemStack());
        SetPointerItem(newPointerItem);

        UpdateInventory();
    }
}