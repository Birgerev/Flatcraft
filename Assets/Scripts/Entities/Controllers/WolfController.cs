using System;
using System.Collections.Generic;

public class WolfController : AnimalController
{
    protected override bool jumpWhenHitting { get; } = true;
    protected override bool targetDamagerIfAttacked { get; } = true;
    protected override float targetSearchRange { get; } = 8;
    protected override float targetLooseRange { get; } = 16;
    protected override List<Type> targetSearchEntityTypes { get; } = new List<Type>() {typeof(Sheep), typeof(Skeleton)};
    protected override float hitTargetDamage { get; } = 6f;
    
    public WolfController(LivingEntity instance) : base(instance)
    {
    }

    public override void Tick()
    {
        base.Tick();

        ((Wolf)instance).visuallyAngry = (target != null);
    }
}