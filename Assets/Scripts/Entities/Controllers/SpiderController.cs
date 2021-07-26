public class SpiderController : MonsterController
{
    protected override bool jumpWhenHitting { get; } = true;
    protected override bool targetDamagerIfAttacked { get; } = true;
    protected override float targetSearchRange { get; } = 12;
    protected override float targetLooseRange { get; } = 18;
    protected override float hitTargetDamage { get; } = 3f;
    
    public SpiderController(LivingEntity instance) : base(instance)
    {
    }
}