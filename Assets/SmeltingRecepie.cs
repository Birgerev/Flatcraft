using System;
using System.Collections.Generic;
using UnityEngine;

public class SmeltingRecepie
{
    public static float smeltTime = 10;

    public static Dictionary<Material, float> Fuels = new Dictionary<Material, float>
    {
        {Material.Coal, 80}
    };

    public Material ingredient = Material.Air;

    public ItemStack result;

    public bool Compare(Material ingredientItem)
    {
        return ingredientItem == ingredient;
    }

    public static SmeltingRecepie FindRecepieByIngredient(Material ingredientItem)
    {
        foreach (var recepie in allRecepies())
            if (recepie.Compare(ingredientItem))
                return recepie;
        return null;
    }

    public static SmeltingRecepie[] allRecepies()
    {
        var files = Resources.LoadAll<TextAsset>("Recepies/Smelting");
        var recepies = new List<SmeltingRecepie>();

        foreach (var file in files) recepies.Add(FileToRecepie(file));

        return recepies.ToArray();
    }

    public static SmeltingRecepie FileToRecepie(TextAsset file)
    {
        var recepie = new SmeltingRecepie();
        var lines = file.text.Split('\n');

        recepie.result = new ItemStack(
            (Material) Enum.Parse(typeof(Material), lines[0].Split('*')[0]),
            int.Parse(lines[0].Split('*')[1]), lines[0].Split('*')[2]);

        recepie.ingredient = (Material) Enum.Parse(typeof(Material), lines[1]);

        return recepie;
    }
}