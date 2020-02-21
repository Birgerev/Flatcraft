using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sheep : PassiveEntity
{
    //Entity Properties
    public override float maxHealth { get; } = 8;

    public override List<ItemStack> GetDrops()
    {
        //Drop a random amount of a certain item
        List<ItemStack> result = new List<ItemStack>();
        System.Random r = new System.Random(Chunk.seedByLocation(location));

        result.Add(new ItemStack(Material.Wool_Block, r.Next(0, 3 + 1)));

        return result;
    }

    public override EntityController GetController()
    {
        return new AnimalController(this);
    }

}