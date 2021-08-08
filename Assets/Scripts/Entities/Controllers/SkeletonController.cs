using UnityEngine;

public class SkeletonController : MonsterController
{
    protected override bool targetDamagerIfAttacked { get; } = true;
    protected override float targetSearchRange { get; } = 12;
    protected override float targetLooseRange { get; } = 18;
    protected override float pathfindingKeepDistanceToTarget { get; } = 10;
    
    public SkeletonController(LivingEntity instance) : base(instance)
    {
    }
    
    
    public override void Tick()
    {
        base.Tick();

        TryShoot();
    }
    
    protected virtual void TryShoot()
    {
        if (target == null || GetTargetDistance() > 12)
            return;

        Skeleton skeleton = (Skeleton) instance;

        if (skeleton.isShooting)
            return;
        
        skeleton.AimAndShootEntity(target);
    }
}