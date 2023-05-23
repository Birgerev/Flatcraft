using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Chicken : PassiveEntity
{
    //Entity Properties
    public override float maxHealth { get; } = 4;

    [Server]
    public override List<ItemStack> GetDrops()
    {
        List<ItemStack> result = new List<ItemStack>();

        result.Add(new ItemStack(Material.Raw_Chicken, 1));

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

    [Server]
    public override EntityController GetController()
    {
        return new AnimalController(this);
    }
}