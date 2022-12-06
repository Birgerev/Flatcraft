using System;
using System.Collections.Generic;

public class Zombie : Monster
{
    public override float maxHealth { get; } = 20;
    protected override bool burnUnderSun { get; } = true;

    public override List<ItemStack> GetDrops()
    {
        //Drop a random amount of a certain item
        List<ItemStack> result = new List<ItemStack>();
        Random r = new Random(SeedGenerator.SeedByWorldLocation(Location));

        result.Add(new ItemStack(Material.Rotten_Flesh, r.Next(0, 2 + 1)));

        return result;
    }

    public override EntityController GetController()
    {
        return new ZombieController(this);
    }
}