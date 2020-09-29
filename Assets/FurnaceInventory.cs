using System;

[Serializable]
public class FurnaceInventory : Inventory
{
    public float fuelLeft;
    public float highestFuel;
    public int smeltingProgress;

    public FurnaceInventory()
    {
        initialize(3, "Furnace");
    }

    public int getFuelSlot()
    {
        return 0;
    }

    public int getIngredientSlot()
    {
        return 1;
    }

    public int getResultSlot()
    {
        return 2;
    }

    public override void UpdateMenuStatus()
    {
        var inventory = InventoryMenuManager.instance.furnaceInventoryMenu;
        inventory.active = open;
        inventory.inventory = this;
    }
}