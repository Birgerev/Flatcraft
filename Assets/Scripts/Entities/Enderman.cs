using System;
using System.Collections.Generic;

public class Enderman : Monster
{
    public override float maxHealth { get; } = 40;

    public override List<ItemStack> GetDrops()
    {
        //Drop a random amount of a certain item
        List<ItemStack> result = new();
        Random r = new(SeedGenerator.SeedByLocation(Location));

        result.Add(new ItemStack(Material.Enderpearl, r.Next(0, 1 + 1)));

        return result;
    }

    public override EntityController GetController()
    {
        return new ZombieController(this);
    }
}