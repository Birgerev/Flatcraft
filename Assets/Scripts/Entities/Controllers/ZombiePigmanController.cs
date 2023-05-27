using System;
using System.Collections.Generic;

public class ZombiePigmanController : MonsterController
{

    protected override List<Type> targetSearchEntityTypes { get; } = new List<Type>() {};
    protected override float targetLooseRange { get; } = 55;
    protected override float hitTargetDamage { get; } = 12f;
    
    public ZombiePigmanController(LivingEntity instance) : base(instance)
    {
    }
    
    //TODO notify frens
    //https://minecraft.fandom.com/wiki/Zombified_Piglin
}