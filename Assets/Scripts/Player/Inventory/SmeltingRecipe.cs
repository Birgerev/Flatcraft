using System;
using System.Collections.Generic;
using UnityEngine;

public class SmeltingRecipe
{
    public static float smeltTime = 10;

    public static Dictionary<Material, float> Fuels = new Dictionary<Material, float>
    {
        {Material.Coal, 80},
        {Material.Lava_Bucket, 1000},
        {Material.Oak_Log, 15},
        {Material.Birch_Log, 15},
        {Material.Oak_Planks, 15},
        {Material.Ladder, 15},
        {Material.Crafting_Table, 15},
        {Material.Chest, 15},
        {Material.Wooden_Axe, 10},
        {Material.Wooden_Pickaxe, 10},
        {Material.Wooden_Hoe, 10},
        {Material.Wooden_Shovel, 10},
        {Material.Wooden_Sword, 10},
        {Material.Oak_Sapling, 5},
    };

    public Material ingredient = Material.Air;

    public ItemStack result;

    public bool Compare(Material ingredientItem)
    {
        return ingredientItem == ingredient;
    }

    public static SmeltingRecipe FindRecipeByIngredient(Material ingredientItem)
    {
        foreach (SmeltingRecipe recipe in allRecipes())
            if (recipe.Compare(ingredientItem))
                return recipe;
        return null;
    }

    public static SmeltingRecipe[] allRecipes()
    {
        TextAsset[] files = Resources.LoadAll<TextAsset>("Recipes/Smelting");
        List<SmeltingRecipe> recipes = new List<SmeltingRecipe>();

        foreach (TextAsset file in files)
            recipes.Add(FileToRecipe(file));

        return recipes.ToArray();
    }

    public static SmeltingRecipe FileToRecipe(TextAsset file)
    {
        SmeltingRecipe recipe = new SmeltingRecipe();
        string[] lines = file.text.Split('\n');

        recipe.result = new ItemStack((Material) Enum.Parse(typeof(Material), lines[0].Split('*')[0]),
            int.Parse(lines[0].Split('*')[1]), lines[0].Split('*')[2]);

        recipe.ingredient = (Material) Enum.Parse(typeof(Material), lines[1]);

        return recipe;
    }
}