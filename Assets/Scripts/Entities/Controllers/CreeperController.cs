using UnityEngine;

public class CreeperController : MonsterController
{
    public CreeperController(LivingEntity instance) : base(instance)
    {
    }

    protected override float targetSearchRange { get; } = 12;
    protected override float targetLooseRange { get; } = 18;

    protected override float pathfindingKeepDistanceToTarget { get; } = 2;
    
    protected virtual float igniteDistance { get; } = 3f;
    protected virtual float defuseDistance { get; } = 6f;

    public override void Tick()
    {
        base.Tick();

        bool isIgnited = ((Creeper)instance).ignited;

        if (isIgnited)
            AttemptToDefuse();
        else 
            AttemptToIgnite();
    }

    protected virtual void AttemptToIgnite()
    {
        if(target == null)
            return;
                
        float distance = Vector2.Distance(target.Location.GetPosition(), instance.Location.GetPosition());
        if (distance > igniteDistance)
            return;

        if (!HasSightline(target.Location))
            return;
        
        ((Creeper) instance).SetIgnited(true);
    }
    
    protected virtual void AttemptToDefuse()
    {
        if(target != null)
        {
            float distance = Vector2.Distance(target.Location.GetPosition(), instance.Location.GetPosition());
            
            if (distance < defuseDistance)
                return;
        }
        
        ((Creeper) instance).SetIgnited(false);
    }

    protected override void Walking()
    {
        //Don't walk when ignited
        if (((Creeper) instance).ignited)
            return;

        base.Walking();
    }
}