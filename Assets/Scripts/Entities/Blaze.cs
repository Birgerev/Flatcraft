using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Blaze : Monster
{
    public override float maxHealth { get; } = 20;

    public override List<ItemStack> GetDrops()
    {
        //Drop a random amount of a certain item
        List<ItemStack> result = new();
        System.Random r = new(SeedGenerator.SeedByWorldLocation(Location));

        result.Add(new ItemStack(Material.Blaze_Rod, r.Next(0, 1 + 1)));

        return result;
    }

    [Server]
    public override void Tick()
    {
        base.Tick();

        //Fall speed
        if (!isOnGround && GetVelocity().y < -1)
            SetVelocity(new Vector2(GetVelocity().x, -1));
    }

    [Server]
    public override void TakeFallDamage(float damage)
    {
        //Disable fall damage
    }

    public override EntityController GetController()
    {
        return new BlazeController(this);
    }
    
    public override void TakeFireDamage(float damage){ } //Disable fire damage
}