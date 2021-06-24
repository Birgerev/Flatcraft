using System;
using System.Collections.Generic;

public class Skeleton : Monster
{
    public override float maxHealth { get; } = 20;
    protected override bool burnUnderSun { get; } = true;

    public override List<ItemStack> GetDrops()
    {
        //Drop a random amount of a certain item
        List<ItemStack> result = new List<ItemStack>();
        Random r = new Random(SeedGenerator.SeedByLocation(Location));

        result.Add(new ItemStack(Material.Bread, r.Next(0, 2 + 1)));

        return result;
    }

    public override EntityController GetController()
    {
        return new SkeletonController(this);
    }
}