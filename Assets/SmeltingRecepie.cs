using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SmeltingRecepie
{
    public static float smeltTime = 10;
    public static Dictionary<Material, float> Fuels = new Dictionary<Material, float>()
    {
        { Material.Coal, 80}
    };

    public ItemStack result;
    public Material ingredient = Material.Air;

    public SmeltingRecepie()
    {

    }

    public bool Compare(Material ingredientItem)
    {
        return (ingredientItem == ingredient);
    }

    public static SmeltingRecepie FindRecepieByIngredient(Material ingredientItem)
    {
        foreach (SmeltingRecepie recepie in allRecepies())
        {
            if (recepie.Compare(ingredientItem))
            {
                return recepie;
            }
        }
        return null;
    }

    public static SmeltingRecepie[] allRecepies()
    {
        TextAsset[] files = Resources.LoadAll<TextAsset>("Recepies/Smelting");
        List<SmeltingRecepie> recepies = new List<SmeltingRecepie>();

        foreach(TextAsset file in files)
        {
            recepies.Add(FileToRecepie(file));
        }

        return recepies.ToArray();
    }

    public static SmeltingRecepie FileToRecepie(TextAsset file)
    {
        SmeltingRecepie recepie = new SmeltingRecepie();
        string[] lines = file.text.Split('\n');

        recepie.result = new ItemStack(
            (Material)System.Enum.Parse(typeof(Material), lines[0].Split('*')[0]),
            int.Parse(lines[0].Split('*')[1]), lines[0].Split('*')[2]);

        recepie.ingredient = (Material)System.Enum.Parse(typeof(Material), lines[1]);

        return recepie;
    }
}
