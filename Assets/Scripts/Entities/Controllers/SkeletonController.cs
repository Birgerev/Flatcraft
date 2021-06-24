public class SkeletonController : MonsterController
{
    protected override bool targetDamagerIfAttacked { get; } = true;
    protected override float targetRange { get; } = 16;
    protected override float pathfindingKeepDistanceToTarget { get; } = 10;
    
    public SkeletonController(LivingEntity instance) : base(instance)
    {
    }
}