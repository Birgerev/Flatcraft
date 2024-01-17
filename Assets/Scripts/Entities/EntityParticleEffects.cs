using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(Entity))]
public class EntityParticleEffects : NetworkBehaviour
{
    private Entity _entity;
    private bool _inLiquidLastFrame;

    // Update is called once per frame
    void Update()
    {
        MovementParticleEffect();
        CheckWaterSplash();
    }
    
    private void CheckWaterSplash()
    {
        if (!isServer) return;
        //Just entered liquid?
        if (!_entity.isInLiquid) return;
        if (_inLiquidLastFrame) return;
        //Minimum fall velocity
        if (_entity.GetVelocity().y > -2) return;
        
        RPC_LiquidSplashEffect();
        
        //Only play splash sound in water
        foreach (Liquid liquid in _entity.GetLiquidBlocksForEntity())
            if (liquid is Water)
            {
                Sound.Play(_entity.Location, "entity/water_splash", SoundType.Entities, 0.75f, 1.25f); //Play splash sound
                break;
            }
    }
    
    private void MovementParticleEffect()
    {
        if (!_entity.isOnGround) return;

        if (Random.value > 1.5f * Time.deltaTime * Mathf.Abs(_entity.GetVelocity().x)) return;
        
        Block blockBeneath = (_entity.Location - new Location(0, 1)).GetBlock();
        if (blockBeneath == null) return;
        Color[] textureColors = blockBeneath.GetColorsInTexture();

        Particle part = Particle.ClientSpawn();
        part.transform.position = transform.position + new Vector3(0, 0.2f);
        part.color = textureColors[Random.Range(0, textureColors.Length)];
        part.doGravity = true;
        part.velocity = new Vector2(
            _entity.GetVelocity().x * -0.3f,
            Mathf.Abs(_entity.GetVelocity().x) * 1.3f * Random.value);
        part.maxAge = .4f + Random.value * .6f;
    }


    [ClientRpc]
    public virtual void RPC_DamageNumberEffect(int damage, Color color)
    {
        Particle.ClientSpawnNumber(transform.position + new Vector3(1, 2), damage, color);
    }
    
    [ClientRpc]
    public virtual void RPC_DeathSmokeEffect()
    {
        Particle.ClientSpawnSmallSmoke(transform.position, Color.white);
    }
    
    [ClientRpc]
    public void RPC_CriticalDamageEffect()
    {
        for (int i = 0; i < Random.Range(2, 8); i++) //SpawnParticles
        {
            Particle part = Particle.ClientSpawn();

            part.transform.position = transform.position + new Vector3(0, 1f);
            part.color = new Color(0.854f, 0.788f, 0.694f);
            part.doGravity = true;
            part.velocity = new Vector2(
                (2f + Random.value) * (Random.Range(0, 2) == 0 ? -1 : 1), 
                4f + Random.value);
            part.maxAge = 1f + Random.value;
            part.maxBounces = 10;
        }
    }

    [ClientRpc]
    public virtual void RPC_LiquidSplashEffect()
    {
        if (_entity.GetLiquidBlocksForEntity().Length == 0) return;

        Color textureColor = _entity.GetLiquidBlocksForEntity()[0].GetColorsInTexture()[0];
        for (int i = 0; i < 8; i++) //Spawn landing partickes
        {
            Particle part = Particle.ClientSpawn();

            part.transform.position = _entity.Location.GetPosition() + new Vector2(0, 0.5f);
            part.color = textureColor;
            part.doGravity = true;
            part.velocity = new Vector2(
                (1f + Random.value) * (Random.Range(0, 2) == 0 ? -1 : 1), 
                3f + Random.value);
            part.maxAge = 1f + Random.value;
            part.maxBounces = 10;
        }
    }

    public virtual void LateUpdate()
    {
        _inLiquidLastFrame = _entity.isInLiquid;
    }
    
    // Start is called before the first frame update
    void Awake()
    {
        _entity = GetComponent<Entity>();
    }
}
