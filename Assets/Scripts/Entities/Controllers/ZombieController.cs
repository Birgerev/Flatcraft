public class ZombieController : MonsterController
{
    public ZombieController(LivingEntity instance) : base(instance)
    {
    }

    protected override bool targetDamagerIfAttacked { get; } = true;
    protected override float targetRange { get; } = 35;
    protected override float hitTargetDamage { get; } = 4.5f;
}