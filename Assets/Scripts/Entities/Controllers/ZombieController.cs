using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieController : MonsterController
{
    protected override bool targetDamagerIfAttacked { get; } = true;
    protected override float targetRange { get; } = 35;
    protected override float hitTargetDamage { get; } = 4.5f;
    
    public ZombieController(LivingEntity instance) : base(instance)
    {
    }
}
