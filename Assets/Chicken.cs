using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chicken : PassiveEntity
{
    //Entity Properties
    public override float maxHealth { get; } = 4;
    
    public override List<ItemStack> GetDrops()
    {
        List<ItemStack> result = new List<ItemStack>();
        System.Random r = new System.Random(Chunk.seedByPosition(Vector2Int.CeilToInt(transform.position)));

        result.Add(new ItemStack(Material.Gravel, r.Next(0, 3 + 1)));

        return result;
    }

    public override EntityController GetController()
    {
        return new AnimalController(this);
    }

}
