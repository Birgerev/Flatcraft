using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : LivingEntity
{
    protected virtual bool burnUnderSun { get; } = false;
    
    public override void Hit(float damage, Entity source)
    {
        base.Hit(damage, source);

        Particle.Spawn_Number(transform.position + new Vector3(1, 2), (int) damage, Color.red);
    }
}
