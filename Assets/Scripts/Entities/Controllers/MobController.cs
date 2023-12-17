using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class MobController : EntityController
{
    public bool isWalking;

    private float lastHitTime;
    public bool swimDown;

    public Entity target;
    public Location pathfindLocation = new Location();
    private Random r;


    protected virtual float startWanderingChance { get; } = 0.2f;
    protected virtual float stopWanderingChance { get; } = 0.4f;

    protected virtual bool targetDamagerIfAttacked { get; } = false;
    protected virtual float targetSearchRange { get; } = 0;
    protected virtual List<Type> targetSearchEntityTypes { get; } = new List<Type>();
    protected virtual float targetLooseRange { get; } = 0;

    protected virtual float pathfindingKeepDistanceToTarget { get; } = 0;

    protected virtual float hitTargetDamage { get; } = 0;
    protected virtual float hitTargetCooldown { get; } = 1;
    protected virtual bool jumpWhenHitting { get; } = false;
    
    public MobController(LivingEntity instance) : base(instance)
    {
        r = new Random(instance.uuid.GetHashCode());
    }
    
    public override void Tick()
    {
        base.Tick();

        CheckTargetDistance();
        if (targetSearchRange != 0)
            SearchTarget();
        if (targetDamagerIfAttacked)
            FindAttackerTarget();

        RemoveCompletedPathfindLocations();
        if (target != null)
            MoveToTarget();
        Pathfind();

        if(isWalking)
            Walking();

        Swim();
        if (hitTargetDamage > 0)
            TryHit();
    }

    protected virtual void RemoveCompletedPathfindLocations()
    {
        if (Vector2.Distance(pathfindLocation.GetPosition(), instance.Location.GetPosition()) < 1)
        {
            pathfindLocation = new Location();
        }
    }

    protected virtual void MoveToTarget()
    {
        pathfindLocation = target.Location;
    }
    
    protected virtual void Pathfind()
    {
        if (pathfindLocation.Equals(default(Location)))
        {
            Wander();
        }
        else
        {
            Location curLocation = instance.Location;

            isWalking = true;
            instance.facingLeft = curLocation.x > pathfindLocation.x;

            if (instance.isInLiquid)
                swimDown = curLocation.y > pathfindLocation.y;

            if (pathfindingKeepDistanceToTarget != 0)
            {
                float distanceToTarget = Vector2.Distance(curLocation.GetPosition(), pathfindLocation.GetPosition());

                if (distanceToTarget <= pathfindingKeepDistanceToTarget)
                    isWalking = false;
            }
        }
    }

    protected virtual void Wander()
    {
        if (r.NextDouble() < (isWalking ? stopWanderingChance : startWanderingChance) / (1.0f / Time.deltaTime))
        {
            isWalking = !isWalking;
            
            if(isWalking)
                instance.facingLeft = r.Next(0, 2) == 0;
        }

        swimDown = false;
    }

    protected virtual void Walking()
    {
        instance.Walk(instance.facingLeft ? -1 : 1);

        Vector3 locInFront = instance.transform.position + new Vector3(
            (instance.facingLeft ? -1 : 1) * ((instance.GetWidth() / 2) + 0.5f),
            0);
        Block blockInFront = Location.LocationByPosition(locInFront).GetBlock();

        //Jump when there is a block in front of entity
        if (blockInFront != null && blockInFront.IsSolid && !instance.isInLiquid)
            instance.Jump();
    }

    protected virtual void Swim()
    {
        if (instance.isInLiquid && !swimDown)
            instance.Jump();
    }

    protected virtual void SearchTarget()
    {
        if (target != null)
            return;

        foreach (Entity e in Entity.entities)
        {
            if (!targetSearchEntityTypes.Contains(e.GetType()) || 
                Vector2.Distance(e.Location.GetPosition(), instance.Location.GetPosition()) > targetSearchRange)
                continue;
            
            if(!HasSightline(e.Location))
                continue;
            
            target = e;
            break;
        }
            
    }

    protected virtual void FindAttackerTarget()
    {
        if (target != null)
            return;

        Entity attacker = instance.lastDamager;

        if (attacker == null)
            return;

        if (Vector2.Distance(attacker.Location.GetPosition(), instance.Location.GetPosition()) > targetLooseRange)
            return;

        target = attacker;
    }

    protected virtual void TryHit()
    {
        if (target == null || Time.time - lastHitTime < hitTargetCooldown)
            return;

        float distance = GetTargetDistance();

        if (jumpWhenHitting && distance < 2f)
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

        if (GetTargetDistance() > targetLooseRange)
        {
            target = null;
            isWalking = false;
        }
    }

    protected virtual float GetTargetDistance()
    {
        return Vector2.Distance(target.Location.GetPosition(), instance.Location.GetPosition());
    }

    protected virtual bool HasSightline(Location target)
    {
        Vector2 currentPosition = (Vector2)instance.transform.position + new Vector2(0, 1);
        target += new Location(0, 1);
        Vector2 targetDirection = target.GetPosition() - currentPosition;
        
        //Check if blocks obstructs sight line
        LayerMask mask = LayerMask.GetMask("Block");

        //If raycasts hit, sight line is obstructed
        return !Physics2D.Raycast(
            currentPosition, targetDirection.normalized, 
            targetDirection.magnitude, mask);
    }
}