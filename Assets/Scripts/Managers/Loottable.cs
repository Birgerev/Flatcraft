using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

[CreateAssetMenu(fileName = "Loottable", menuName = "ScriptableObjects/Loottable")]
public class Loottable : ScriptableObject
{
    public int minimunRolls;
    public int maximumRolls;
    public List<ItemStack> confirmedItems = new List<ItemStack>();
    public List<LoottableItem> rollItems = new List<LoottableItem>();

    private Random random = new Random();
    
    public static Loottable Load(string name)
    {
        return Resources.Load<Loottable>("Loottables/" + name);
    }

    public List<ItemStack> GetRandomItems()
    {
        int rollAmounts = random.Next(minimunRolls, maximumRolls + 1);
        List<ItemStack> results = new List<ItemStack>(confirmedItems);

        for (int roll = 0; roll < rollAmounts; roll++)
        {
            LoottableItem loottableItem = rollItems[random.Next(0, rollItems.Count)];
            
            if(random.NextDouble() > loottableItem.chance)
                continue;
            
            ItemStack item = new ItemStack(loottableItem.material, 
            random.Next(loottableItem.minAmount, loottableItem.maxAmount));
            results.Add(item);
        }

        return results;
    }
}

[Serializable]
public struct LoottableItem
{
    public Material material;
    public int minAmount;
    public int maxAmount;
    public float chance;
}