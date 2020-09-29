using System;
using System.Collections.Generic;

public class Sheep : PassiveEntity
{
    //Entity Properties
    public override float maxHealth { get; } = 8;

    public override List<ItemStack> GetDrops()
    {
        //Drop a random amount of a certain item
        var result = new List<ItemStack>();
        var r = new Random(SeedGenerator.SeedByLocation(Location));

        result.Add(new ItemStack(Material.Wool_Block, r.Next(0, 3 + 1)));

        return result;
    }

    public override EntityController GetController()
    {
        return new AnimalController(this);
    }
}