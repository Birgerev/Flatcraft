public class ZombieController : MonsterController
{

    protected override bool targetDamagerIfAttacked { get; } = true;
    protected override float targetRange { get; } = 16;
    protected override float hitTargetDamage { get; } = 4.5f;
    
    public ZombieController(LivingEntity instance) : base(instance)
    {
    }
}