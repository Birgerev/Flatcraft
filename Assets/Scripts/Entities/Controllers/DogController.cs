using UnityEngine;

public class DogController : AnimalController
{
    protected override bool jumpWhenHitting { get; } = true;
    protected override float hitTargetDamage { get; } = 6f;
    protected override float targetLooseRange { get; } = 10;
    protected virtual float startFollowOwnerRange { get; } = 5;
    protected virtual float teleportToOwnerRange { get; } = 20;
    
    public DogController(LivingEntity instance) : base(instance)
    {
        
    }
    
    public override void Tick()
    {
        FollowOwner();
        TeleportToOwner();
        FindOwnerTargets();
        
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
        if (Time.time % 1f - Time.deltaTime > 0 || ((Dog) instance).sitting)
            return;
        
        Entity owner = GetOwner();
        if (owner == null)
            return;

        float distance = Vector2.Distance(owner.Location.GetPosition(), instance.Location.GetPosition());

        if (distance < teleportToOwnerRange)
            return;

        instance.Teleport(owner.Location);
    }

    protected virtual void FindOwnerTargets()
    {
        if (Time.time % 1f - Time.deltaTime > 0)    //Only look for a target every x seconds, to optimise performance
            return;
        
        Entity owner = GetOwner();
        if (owner == null)  //return if dogs owner entity isnt present (isnt logged in, or is dog corrupted, etc)
            return;
        if (target != null) //Dont try and find a target if we already have a target
            return;
        
        //Try target dogs last attacker
        if(instance.lastDamager != null)
            if (TryTarget(instance.lastDamager))
                return;

        //Try target dogs owners last attacker
        if (owner.lastDamager != null)
            if(TryTarget(owner.lastDamager))
                return;

        //Try target entities that attacked dogs owner
        foreach (Entity e in Entity.entities)
            if (e.lastDamager == owner)
                if (TryTarget(e))
                    return;
    }
    
    private bool TryTarget(Entity newTarget)
    {
        if (newTarget is LivingEntity && !(newTarget is Dog) && newTarget != GetOwner())
        {
            target = newTarget;
            return true;
        }

        return false;
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