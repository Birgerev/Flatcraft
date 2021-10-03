using System;
using Mirror;
using UnityEngine;

[Serializable]
public class FurnaceInventory : Inventory
{
    [SyncVar] public float fuelLeft;

    [SyncVar] public float highestFuel;

    [SyncVar] public int smeltingProgress;

    public override void Update()
    {
        base.Update();

        if (Time.time % 1f - Time.deltaTime <= 0)
        {
            CheckFuels();
            SmeltTick();
        }
    }

    [Server]
    public static Inventory CreatePreset()
    {
        return Create("FurnaceInventory", 3, "Furnace");
    }

    public int GetFuelSlot()
    {
        return 0;
    }

    public int GetIngredientSlot()
    {
        return 1;
    }

    public int GetResultSlot()
    {
        return 2;
    }

    public void CheckFuels()
    {
        if (fuelLeft <= 0 && GetRecipe() != null &&
            SmeltingRecipe.Fuels.ContainsKey(GetItem(GetFuelSlot()).material))
        {
            fuelLeft = SmeltingRecipe.Fuels[GetItem(GetFuelSlot()).material];
            highestFuel = fuelLeft;

            ItemStack newFuelItem = GetItem(GetFuelSlot());
            newFuelItem.Amount--;
            SetItem(GetFuelSlot(), newFuelItem);
        }
    }

    public void SmeltTick()
    {
        SmeltingRecipe curRecipe = GetRecipe();

        if (fuelLeft <= 0)
            highestFuel = 0;

        if (curRecipe != null && GetItem(GetIngredientSlot()).Amount > 0 &&
            (GetItem(GetResultSlot()).material == curRecipe.result.material ||
             GetItem(GetResultSlot()).material == Material.Air))
        {
            //subtract fuel
            if (fuelLeft > 0)
                fuelLeft--;
            else
                return;

            //Add progress to smeltbar
            smeltingProgress += 1;

            //If smelting is done, give result
            if (smeltingProgress >= SmeltingRecipe.smeltTime)
                FillSmeltingResult();
        }
        else
        {
            //Continiue to deplete fuel if we've already begun smelting but ingredient item was removed
            if (fuelLeft > 0)
                fuelLeft--;

            //Reset smelting progress if item is removed
            smeltingProgress = 0;
        }
    }

    public void FillSmeltingResult()
    {
        //Called once smelting is done 
        SmeltingRecipe curRecepie = GetRecipe();
        ItemStack newResultItem = GetItem(GetResultSlot());
        ItemStack newIngredientItem = GetItem(GetIngredientSlot());
            
        newResultItem.material = curRecepie.result.material;
        newResultItem.Amount += curRecepie.result.Amount;
        newIngredientItem.Amount--;

        SetItem(GetResultSlot(), newResultItem);
        SetItem(GetIngredientSlot(), newIngredientItem);
        
        smeltingProgress = 0;
    }

    public SmeltingRecipe GetRecipe()
    {
        if (GetItem(GetIngredientSlot()).Amount <= 0)
            return null;

        //Get recepie based on contents of ingredient slot
        return SmeltingRecipe.FindRecipeByIngredient(GetItem(GetIngredientSlot()).material);
    }
}