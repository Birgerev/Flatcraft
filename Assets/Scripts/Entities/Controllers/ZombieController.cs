public class ZombieController : MonsterController
{
    protected override float targetSearchRange { get; } = 16;
    protected override float targetLooseRange { get; } = 20;
    protected override float hitTargetDamage { get; } = 4.5f;
    
    public ZombieController(LivingEntity instance) : base(instance)
    {
    }
}