using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HostileEntity : LivingEntity
{
    public override void Hit(float damage)
    {
        base.Hit(damage);

        Particle.Spawn_Number(transform.position + new Vector3(1, 2), (int)damage, Color.red);
    }
}
