using UnityEngine;

public class CreeperController : MonsterController
{
    public CreeperController(LivingEntity instance) : base(instance)
    {
    }

    protected override bool targetDamagerIfAttacked { get; } = true;
    protected override float targetRange { get; } = 16;

    protected override float pathfindingKeepDistanceToTarget { get; } = 2;

    public override void Tick()
    {
        base.Tick();

        CheckIgniteState();
    }

    protected virtual void CheckIgniteState()
    {
        bool setIgnited = false;

        if (target != null)
        {
            float distance = Vector2.Distance(target.Location.GetPosition(), instance.Location.GetPosition());

            if (distance <= 2.5f)
                setIgnited = true;
        }

        if (((Creeper) instance).ignited != setIgnited)
            ((Creeper) instance).SetIgnited(setIgnited);
    }

    protected override void Walking()
    {
        if (((Creeper) instance).ignited)
            return;

        base.Walking();
    }
}