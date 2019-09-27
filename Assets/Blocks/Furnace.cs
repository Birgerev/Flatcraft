using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Furnace : InventoryContainer
{
    public static string default_texture = "block_furnace";
    public override bool playerCollide { get; } = false;
    public override float breakTime { get; } = 6;

    public override Tool_Type propperToolType { get; } = Tool_Type.Pickaxe;
    public override Tool_Level propperToolLevel { get; } = Tool_Level.Wooden;

    public override System.Type inventoryType { get; } = typeof(FurnaceInventory);

    public override void Tick()
    {
        if (inventory == null)
        {
            base.Tick();
            return;
        }
        CheckFuels();
        SmeltTick();

        base.Tick();
    }

    public void CheckFuels()
    {
        if (getInventory().fuelLeft <= 0)
        {
            if (getInventory().getItem(getInventory().getFuelSlot()) != null)
            {
                if (SmeltingRecepie.Fuels.ContainsKey(getInventory().getItem(getInventory().getFuelSlot()).material))
                {
                    if (GetRecepie() != null)
                    {
                        getInventory().fuelLeft = SmeltingRecepie.Fuels[(getInventory().getItem(getInventory().getFuelSlot()).material)];
                        getInventory().highestFuel = getInventory().fuelLeft;
                        getInventory().getItem(getInventory().getFuelSlot()).amount--;
                    }
                }
            }
        }
    }

    public void SmeltTick()
    {
        SmeltingRecepie curRecepie = GetRecepie();

        if (getInventory().fuelLeft <= 0)
            getInventory().highestFuel = 0;

        if (curRecepie != null && getInventory().getItem(getInventory().getIngredientSlot()).amount > 0 &&
                (getInventory().getItem(getInventory().getResultSlot()).material == curRecepie.result.material ||
getInventory().getItem(getInventory().getResultSlot()).material == Material.Air))
        {
            //Add progress to smeltbar
            getInventory().smeltingProgress += 1 / Chunk.TickRate;

            //If smelting is done, give result
            if (getInventory().smeltingProgress >= SmeltingRecepie.smeltTime)
                FillSmeltingResult();

            //subtract fuel
            if (getInventory().fuelLeft > 0)
                getInventory().fuelLeft--;
        }
        else
        {
            //Continiue to deplete fuel if we've already begun smelting but ingredient item was removed
            if (getInventory().fuelLeft < getInventory().highestFuel)
                if (getInventory().fuelLeft > 0)
                    getInventory().fuelLeft--;

            //Reset smelting progress if item is removed
            getInventory().smeltingProgress = 0;
        }
    }

    public void FillSmeltingResult()
    {
        //Called once smelting is done 
        SmeltingRecepie curRecepie = GetRecepie();

        getInventory().getItem(getInventory().getResultSlot()).material = curRecepie.result.material;
        getInventory().getItem(getInventory().getResultSlot()).amount += curRecepie.result.amount;
        getInventory().getItem(getInventory().getIngredientSlot()).amount--;

        getInventory().smeltingProgress = 0;
    }

    public SmeltingRecepie GetRecepie()
    {
        if (getInventory().getItem(getInventory().getIngredientSlot()).amount <= 0)
            return null;
        //Get recepie based on contents of ingredient slot
        return SmeltingRecepie.FindRecepieByIngredient(getInventory().getItem(getInventory().getIngredientSlot()).material);
    }

    private FurnaceInventory getInventory()
    {
        return ((FurnaceInventory)inventory);
    }
}
