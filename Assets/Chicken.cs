using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chicken : PassiveEntity
{
    //Entity Properties
    public override float maxHealth { get; } = 4;
    
    public override List<ItemStack> GetDrops()
    {
        List<ItemStack> result = new List<ItemStack>();
        System.Random r = new System.Random(Chunk.seedByPosition(Vector2Int.CeilToInt(transform.position)));

        result.Add(new ItemStack(Material.Raw_Chicken, r.Next(0, 1 + 1)));

        return result;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        
        
        if (!isOnGround && getVelocity().y < -1)
            setVelocity(new Vector2(getVelocity().x, -1));
    }

    public override void TakeFallDamage(float damage)
    {
    }

    public override EntityController GetController()
    {
        return new AnimalController(this);
    }

}
