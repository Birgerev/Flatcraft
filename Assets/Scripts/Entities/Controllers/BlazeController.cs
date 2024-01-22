public class BlazeController : MonsterController
{
    protected override float targetSearchRange { get; } = 48;
    protected override float targetLooseRange { get; } = 50;
    
    protected int maxFlyHeightAboveTarget { get; } = 4;
    
    public BlazeController(LivingEntity instance) : base(instance)
    {
        //TODO fireballs
    }

    public override void Tick()
    {
        base.Tick();

        if (target != null)
        {
            instance.fireTime = 1;
            
            //If were not 4 blocks above target, fly upwards
            if(instance.Location.y - target.Location.y  < maxFlyHeightAboveTarget)
                ((Blaze)instance).Fly();
        }
    }
    
    protected override void MoveToTarget() { }//DIsable moving towards target
}