using System;
using System.Collections.Generic;

public class MonsterController : MobController
{
    protected override List<Type> targetSearchEntityTypes { get; } = new List<Type>() {typeof(Player)};
    
    public MonsterController(LivingEntity instance) : base(instance)
    {
    }
}