using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.Mathematics;
using UnityEngine;
using Random = System.Random;

public class EnderEyeEntity :  Entity
{
    private const double ShatterRate = 0.2d;
    
    private const float MovementAge = 4;
    private const float DeathAge = 7; 
    
    private const int TravelDownwardsWithinDistance = 24;
    private const int HorizontalTravelDistance = 12;
    
    private const float HorizontalVelocity = 10;
    private const float UpwardsVelocity = 4;
    private const float DownwardsVelocity = -8;
    
    private int _targetX;
    private bool _willShatter;
    
    [Server]
    public override void Spawn()
    {
        base.Spawn();
        
        //Explode in wrong dimension
        if(Location.dimension != Dimension.Overworld)
            Die();
        
        _targetX = OverworldGenerator.GetStrongholdLocation();
        _willShatter = new Random().NextDouble() < ShatterRate;
    }

    [Server]
    public override void Tick()
    {
        base.Tick();

        if(age <= MovementAge)
            Movement();
        
        if(age >= DeathAge)
            Die();
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

    public override void Die()
    {
        base.Die();
        
        if(_willShatter)
            EyeShatterEffect();
    }

    public override List<ItemStack> GetDrops()
    {
        List<ItemStack> result = new();
        
        if(!_willShatter)
            result.Add(new ItemStack(Material.Eye_Of_Ender, 1));

        return result;
    }

    [ClientRpc]
    public void EyeShatterEffect()
    {
        Particle.ClientSpawnEnderShatter(Location.GetPosition(), new Color(.8f, .25f, .8f));
    }

    public override void TakeSuffocationDamage(float damage) { }//Disable suffocation damage
    
    [Server]
    public override void Hit(float damage, Entity source) { }//Disabling Hit
}
