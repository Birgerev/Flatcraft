using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.Mathematics;
using UnityEngine;

public class EnderEyeEntity :  Entity
{
    //TODO private const
    public int TravelDownwardsWithinDistance = 24;
    public int HorizontalTravelDistance = 12;
    public float HorizontalVelocity = 4;
    public float UpwardsVelocity = 2;
    public float DownwardsVelocity = -2;
    
    private int targetX;
    
    [Server]
    public override void Spawn()
    {
        base.Spawn();

        
        targetX = 0;
    }

    [Server]
    public override void Tick()
    {
        base.Tick();

        int offsetToTarget = targetX - Location.x;
        int distanceToTarget = Mathf.Abs(offsetToTarget);
        
        bool targetIsRight = (offsetToTarget > 0);
        bool doDownwardsTravel = (distanceToTarget < TravelDownwardsWithinDistance);
        bool doHorizontalTravel = (distanceToTarget > HorizontalTravelDistance);
        
        Vector2 newVelocity = Vector2.zero;
        newVelocity.y = doDownwardsTravel ? DownwardsVelocity : UpwardsVelocity;
        if(doHorizontalTravel)
            newVelocity.x = targetIsRight ? HorizontalVelocity : -HorizontalVelocity;
        
        //TODO particles
        //TODO drop
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
