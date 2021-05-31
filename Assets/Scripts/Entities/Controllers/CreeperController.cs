using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreeperController : MonsterController
{
    protected override bool targetDamagerIfAttacked { get; } = true;
    protected override float targetRange { get; } = 16;
    
    protected override float pathfindingKeepDistanceToTarget { get; } = 2;
    
    public CreeperController(LivingEntity instance) : base(instance)
    {
    }

    public override void Tick()
    {
        base.Tick();
        
        CheckIgniteState();
    }
    
    protected virtual void CheckIgniteState()
    {
        bool ignited = false;
        
        if (target != null)
        {
            float distance = Vector2.Distance(target.Location.GetPosition(), instance.Location.GetPosition());

            if (distance <= 2.5f)
            {
                ignited = true;
            }
        }
        
        ((Creeper) instance).SetIgnited(ignited);
    }
    
    protected override void Walking()
    {
        if (((Creeper) instance).ignited)
            return;
        
        base.Walking();
    }
}
