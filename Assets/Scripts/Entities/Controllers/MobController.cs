using UnityEngine;
using Random = System.Random;

public class MobController : EntityController
{
    protected virtual float startWanderingChance { get; } = 0.2f;
    protected virtual float stopWanderingChance { get; } = 0.4f;
    
    protected virtual bool targetDamagerIfAttacked { get; } = false;
    protected virtual float targetRange { get; } = 0;
    
    protected virtual float pathfindingKeepDistanceToTarget { get; } = 0;
    
    protected virtual float hitTargetDamage { get; } = 0;
    protected virtual float hitTargetCooldown { get; } = 1;
    protected virtual bool jumpWhenHitting { get; } = false;
    
    public Entity target;
    public bool isWalking;
    public bool walkingRight;
    public bool swimDown;
    
    private float lastHitTime;
    
    public MobController(LivingEntity instance) : base(instance)
    {
    }
    
    public override void Tick()
    {
        base.Tick();
        
        CheckTargetDistance();
        if(targetRange != 0)
            FindPlayerTarget();
        if (targetDamagerIfAttacked)
            FindAttackerTarget();
        Pathfind();
        
        Walking();
        Swim();
        if (hitTargetDamage != 0)
            TryHit();
    }

    protected virtual void Pathfind()
    {
        Location pathfindLocation = new Location();

        if (target != null)
        {
            pathfindLocation = target.Location;
        }
        //TODO else linger location

        if (pathfindLocation.Equals(default(Location)))
        {
            Wander();
        }
        else
        {
            Location curLocation = instance.Location;

            isWalking = true;
            walkingRight = (curLocation.x < pathfindLocation.x);

            if (instance.isInLiquid)
            {
                swimDown = (curLocation.y > pathfindLocation.y);
            }

            if (pathfindingKeepDistanceToTarget != 0)
            {
                float distanceToTarget = Vector2.Distance(curLocation.GetPosition(), pathfindLocation.GetPosition());

                if (distanceToTarget <= pathfindingKeepDistanceToTarget)
                {
                    isWalking = false;
                }
            }
        }
        
    }

    protected virtual void Wander()
    {
        var r = new Random((instance.Location + " " + instance.age).GetHashCode());

        if (r.NextDouble() < (isWalking ? stopWanderingChance : startWanderingChance) / (1.0f / Time.deltaTime))
        {
            isWalking = !isWalking;
            walkingRight = r.Next(0, 2) == 0;
        }

        swimDown = false;
    }

    protected virtual void Walking()
    {
        if (isWalking)
        {
            instance.Walk(walkingRight ? 1 : -1);

            Block blockInFront = (instance.Location + new Location(walkingRight ? 1 : -1, 0)).GetBlock();
            
            //Jump when there is a block in front of entity
            if (blockInFront != null && blockInFront.solid && !instance.isInLiquid) 
                instance.Jump();
        }
    }

    protected virtual void Swim()
    {
        if (instance.isInLiquid && !swimDown)
            instance.Jump();
    }
    
    protected virtual void FindPlayerTarget()
    {
        if (target != null)
            return;
        
        foreach (Player p in Player.players)
        {
            if (Vector2.Distance(p.Location.GetPosition(), instance.Location.GetPosition()) < targetRange)
            {
                target = p;
                break;
            } 
        }
    }

    protected virtual void FindAttackerTarget()
    {
        if (target != null)
            return;

        Entity attacker = instance.lastDamager;

        if (attacker == null)
            return;

        if (Vector2.Distance(attacker.Location.GetPosition(), instance.Location.GetPosition()) > targetRange)
            return;

        target = attacker;
    }
    
    protected virtual void TryHit()
    {
        if (target == null || Time.time - lastHitTime < hitTargetCooldown)
            return;

        float distance = Vector2.Distance(target.Location.GetPosition(), instance.Location.GetPosition());
            
        if(jumpWhenHitting && distance < 2f)
            instance.Jump();
        
        if (distance < 1.5f)
        {
            target.Hit(hitTargetDamage, instance);
            lastHitTime = Time.time;
        }
    }
    
    protected virtual void CheckTargetDistance()
    {
        if (target == null)
            return;

        if (Vector2.Distance(target.Location.GetPosition(), instance.Location.GetPosition()) > targetRange)
        {
            target = null;
            isWalking = false;
        }
    }
}
