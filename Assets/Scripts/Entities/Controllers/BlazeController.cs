public class BlazeController : MonsterController
{
    protected override bool targetDamagerIfAttacked { get; } = true;
    protected override float targetSearchRange { get; } = 48;
    protected override float targetLooseRange { get; } = 50;
    
    public BlazeController(LivingEntity instance) : base(instance)
    {
    }
    
    //https://minecraft.fandom.com/wiki/Blaze
}