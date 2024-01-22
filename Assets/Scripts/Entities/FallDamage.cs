using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

public class FallDamage : NetworkBehaviour
{
    [HideInInspector] public float lastGroundedHeight;
    
    private Entity _entity;
    
    // Update is called once per frame
    void Update()
    {
        if (!isServer) return;
        
        FallDamageCheck();
        UpdateLastHeight();
    }
    
    [Server]
    private void FallDamageCheck()
    {
        if (!_entity.isOnGround) return;
        if (_entity.isInLiquid) return;
        if (_entity.age < 5) return;
        
        float damage = lastGroundedHeight - transform.position.y - 3;
        if (damage < 1) return;
        
        Sound.Play(_entity.Location, "entity/land", SoundType.Entities, 0.5f, 1.5f); //Play entity land sound
        _entity.Damage(damage);
                
        Block blockBelow = Location.LocationByPosition(transform.position - new Vector3(0, 0.2f)).GetBlock();
        if (!blockBelow) return;
        
        RPC_FallParticleEffect(blockBelow.location);
    }

    [Server]
    private void UpdateLastHeight()
    {
        if (_entity.isOnGround || 
            _entity.isInLiquid || 
            (_entity.GetComponent<LivingEntity>() && _entity.GetComponent<LivingEntity>().isOnClimbable) ||
            transform.position.y > lastGroundedHeight)
            lastGroundedHeight = transform.position.y;
    }

    [ClientRpc]
    private void RPC_FallParticleEffect(Location groundLocation)
    {
        Block blockBelow = groundLocation.GetBlock();
        Color[] textureColors = blockBelow.GetColorsInTexture();
        System.Random r = new System.Random();

        //Spawn landing particles
        for (int i = 0; i < 10; i++) 
        {
            Particle part = Particle.ClientSpawn();

            part.transform.position = blockBelow.location.GetPosition() + new Vector2(0, 0.6f);
            part.color = textureColors[r.Next(textureColors.Length)];
            part.doGravity = true;
            part.velocity = new Vector2(((float) r.NextDouble() - 0.5f) * 2, 1.5f);
            part.maxAge = 1f + (float) r.NextDouble();
            part.maxBounces = 10;
        }
    }

    private void Awake()
    {
        _entity = GetComponent<Entity>();
    }
}
