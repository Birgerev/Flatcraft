public class SpiderController : MonsterController
{
    protected override bool jumpWhenHitting { get; } = true;
    protected override bool targetDamagerIfAttacked { get; } = true;
    protected override float targetRange { get; } = 12;
    protected override float hitTargetDamage { get; } = 3f;
    
    public SpiderController(LivingEntity instance) : base(instance)
    {
    }
}