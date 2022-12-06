using System;
using System.Collections.Generic;

public class EndermanController : MonsterController
{
    protected override List<Type> targetSearchEntityTypes { get; } = new List<Type>() {};
    
    protected override bool targetDamagerIfAttacked { get; } = true;
    protected override float targetLooseRange { get; } = 100;
    protected override float hitTargetDamage { get; } = 10.5f;
    
    public EndermanController(LivingEntity instance) : base(instance)
    {
    }
    
    //TODO run speed
    //TODO teleport
    //https://minecraft.fandom.com/wiki/Enderman
}