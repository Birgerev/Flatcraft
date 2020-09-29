using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class Chicken : PassiveEntity
{
    //Entity Properties
    public override float maxHealth { get; } = 4;

    public override List<ItemStack> GetDrops()
    {
        var result = new List<ItemStack>();
        var r = new Random(SeedGenerator.SeedByLocation(Location));

        result.Add(new ItemStack(Material.Raw_Chicken, r.Next(0, 1 + 1)));

        return result;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();


        if (!isOnGround && GetVelocity().y < -1)
            SetVelocity(new Vector2(GetVelocity().x, -1));
    }

    public override void TakeFallDamage(float damage)
    {
    }

    public override EntityController GetController()
    {
        return new AnimalController(this);
    }
}