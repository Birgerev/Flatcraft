using System;
using System.Collections.Generic;
using UnityEngine;

public class SmeltingRecipe
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

    public static SmeltingRecipe FindRecipeByIngredient(Material ingredientItem)
    {
        foreach (var recipe in allRecipes())
            if (recipe.Compare(ingredientItem))
                return recipe;
        return null;
    }

    public static SmeltingRecipe[] allRecipes()
    {
        var files = Resources.LoadAll<TextAsset>("Recipes/Smelting");
        var recipes = new List<SmeltingRecipe>();

        foreach (var file in files) 
            recipes.Add(FileToRecipe(file));

        return recipes.ToArray();
    }

    public static SmeltingRecipe FileToRecipe(TextAsset file)
    {
        var recipe = new SmeltingRecipe();
        var lines = file.text.Split('\n');

        recipe.result = new ItemStack(
            (Material) Enum.Parse(typeof(Material), lines[0].Split('*')[0]),
            int.Parse(lines[0].Split('*')[1]), lines[0].Split('*')[2]);

        recipe.ingredient = (Material) Enum.Parse(typeof(Material), lines[1]);

        return recipe;
    }
}