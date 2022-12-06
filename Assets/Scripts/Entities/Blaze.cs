using System;
using System.Collections.Generic;

public class Blaze : Monster
{
    public override float maxHealth { get; } = 20;

    public override List<ItemStack> GetDrops()
    {
        //Drop a random amount of a certain item
        List<ItemStack> result = new();
        Random r = new(SeedGenerator.SeedByLocation(Location));

        result.Add(new ItemStack(Material.Blaze_Rod, r.Next(0, 1 + 1)));

        return result;
    }

    public override EntityController GetController()
    {
        return new BlazeController(this);
    }
    
    public override void TakeFireDamage(float damage){ } //Disable fire damage
    public override void TakeLavaDamage(float damage){ } //Disable lava damage
}