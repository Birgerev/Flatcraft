using UnityEngine;

public class DogController : AnimalController
{
    protected override bool jumpWhenHitting { get; } = true;
    protected override float hitTargetDamage { get; } = 6f;
    protected virtual float startFollowOwnerRange { get; } = 5;
    protected virtual float teleportToOwnerRange { get; } = 20;
    
    public DogController(LivingEntity instance) : base(instance)
    {
        
    }
    
    public override void Tick()
    {
        FollowOwner();
        TeleportToOwner();
        
        base.Tick();
    }

    protected virtual void FollowOwner()
    {
        Entity owner = GetOwner();
        if (owner == null)
            return;

        float distance = Vector2.Distance(owner.Location.GetPosition(), instance.Location.GetPosition());

        if (distance < startFollowOwnerRange)
            return;

        pathfindLocation = owner.Location;
    }

    protected virtual void TeleportToOwner()
    {
        if (((Dog) instance).sitting)
            return;
        
        Entity owner = GetOwner();
        if (owner == null)
            return;

        float distance = Vector2.Distance(owner.Location.GetPosition(), instance.Location.GetPosition());

        if (distance < teleportToOwnerRange)
            return;

        instance.Teleport(owner.Location);
    }
    
    protected virtual Entity GetOwner()
    {
        Dog dog = (Dog) instance;
        Entity owner = Entity.GetEntity(dog.ownerUuid);

        return owner;
    }

    
    protected override void Walking()
    {
        if (((Dog) instance).sitting)
            return;
        
        base.Walking();
    }
}