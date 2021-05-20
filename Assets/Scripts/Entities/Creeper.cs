using System;
using System.Collections.Generic;

public class Creeper : Monster
{
    public override float maxHealth { get; } = 10;

    public override List<ItemStack> GetDrops()
    {
        //Drop a random amount of a certain item
        List<ItemStack> result = new List<ItemStack>();
        Random r = new Random(SeedGenerator.SeedByLocation(Location));

        result.Add(new ItemStack(Material.Gunpowder, r.Next(0, 2 + 1)));

        return result;
    }

    public override EntityController GetController()
    {
        return new CreeperController(this);
    }
}
