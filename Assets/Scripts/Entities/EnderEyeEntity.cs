using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.Mathematics;
using UnityEngine;

public class EnderEyeEntity :  Entity
{
    
    
    private const float HorizontalVelocity = 10;
    private const float UpwardsVelocity = 4;
    private const float DownwardsVelocity = -8;
    
    private int _targetX;
    
    [Server]
    public override void Spawn()
    {
        base.Spawn();

        _targetX = 0;
    }

    [Server]
    public override void Tick()
    {
        base.Tick();

            Movement();
        
        if(age >= DeathAge)
            Die();

        //TODO particles
        //TODO random drop rate
        //TODO proper target
        //TODO explode in wrong dimension
    }

    private void Movement()
    {
        int offsetToTarget = _targetX - Location.x;
        int distanceToTarget = Mathf.Abs(offsetToTarget);
        
        bool targetIsRight = (offsetToTarget > 0);
        bool doDownwardsTravel = (distanceToTarget < TravelDownwardsWithinDistance);
        bool doHorizontalTravel = (distanceToTarget > HorizontalTravelDistance);
        
        Vector2 newVelocity = Vector2.zero;
        newVelocity.y = doDownwardsTravel ? DownwardsVelocity : UpwardsVelocity;
        if(doHorizontalTravel)
            newVelocity.x = targetIsRight ? HorizontalVelocity : -HorizontalVelocity;
        
        GetComponent<Rigidbody2D>().velocity += newVelocity * Time.deltaTime;
    }
    
    public override List<ItemStack> GetDrops()
    {
        //TODO random return
        List<ItemStack> result = new List<ItemStack>();
        result.Add(new ItemStack(Material.Eye_Of_Ender, 1));

        return result;
    }

    public override void TakeSuffocationDamage(float damage) { }//Disable suffocation damage
    
    [Server]
    public override void Hit(float damage, Entity source) { }//Disabling Hit
}
