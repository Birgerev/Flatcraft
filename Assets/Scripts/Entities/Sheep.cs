using System;
using System.Collections.Generic;

public class Sheep : PassiveEntity
{
    //Entity Properties
    public override float maxHealth { get; } = 8;

    public override List<ItemStack> GetDrops()
    {
        //Drop a random amount of a certain item
        List<ItemStack> result = new List<ItemStack>();
        Random r = new Random(SeedGenerator.SeedByWorldLocation(Location));

        result.Add(new ItemStack(Material.White_Wool, r.Next(0, 3 + 1)));

        return result;
    }

    public override EntityController GetController()
    {
        return new AnimalController(this);
    }
}