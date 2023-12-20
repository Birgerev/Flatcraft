using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class EntityMovement : NetworkBehaviour
{
    private const float JumpVelocity = 8.5f;
    private const float StepSoundDistance = .9f;
    
    [Header("Movement Properties")] 
    private const float AirDrag = 4.3f;
    private const float ClimbableFriction = 10f;
    private const float GroundFriction = 5f;
    private const float LiquidDrag = 10f;
    
    private const float Acceleration = 4f;
    private const float ClimbAcceleration = 55.0f;
    private const float ExitLiquidJumpVelocity = 2f;
    private const float SwimUpAcceleration = 45.0f;
    
    public float walkSpeed = 4.3f;
    
    [HideInInspector] public float speed;
    
    private float _lastJumpTime;
    private Vector2 _lastStepPosition;
    private bool _inLiquidLastFrame;
    
    private LivingEntity _entity;
    private Rigidbody2D _rb;


    // Update is called once per frame
    void Update()
    {
        MovementParticleEffect();
        
        if (!isOwned && !isServer) return;
        
        WalkSound();
        ApplyFriction();
        
        //Exit liquid jump
        if (_inLiquidLastFrame && !_entity.isInLiquid && _rb.velocity.y > 0)
            _rb.velocity += new Vector2(0, ExitLiquidJumpVelocity);
    }

    private void ApplyFriction()
    {
        if (!_entity.isInLiquid && !_entity.isOnClimbable && !_entity.isOnGround)
            _rb.velocity = new Vector3(_rb.velocity.x * (1 / (1 + AirDrag * Time.deltaTime)), _rb.velocity.y);
        
        if (_entity.isInLiquid)
            _rb.velocity *= 1 / (1 + LiquidDrag * Time.deltaTime);
        if (_entity.isOnClimbable)
            _rb.velocity *= 1 / (1 + ClimbableFriction * Time.deltaTime);
        if (!_entity.isInLiquid && !_entity.isOnClimbable && _entity.isOnGround)
            _rb.velocity *= 1 / (1 + GroundFriction * Time.deltaTime);
    }

    public void Walk(int direction)
    {
        if (!isOwned && !isServer)
            return;
        
        StairCheck(direction);
        
        if (Mathf.Abs(_rb.velocity.x) > speed) return;

        GetComponent<Rigidbody2D>().velocity +=
            new Vector2(direction * speed * Acceleration * Time.deltaTime, 0);
    }

    public void Jump()
    {
        if (!isOwned && !isServer) return;

        if (_entity.isInLiquid)
            _rb.velocity += new Vector2(0, SwimUpAcceleration * Time.deltaTime);

        if (_entity.isOnClimbable)
            _rb.velocity += new Vector2(0, ClimbAcceleration * Time.deltaTime);

        if (!_entity.isOnGround) return;
        if (Time.time - _lastJumpTime < .3f) return;
        
        _rb.velocity += new Vector2(0, JumpVelocity);
        _lastJumpTime = Time.time;
    }
    
    private void StairCheck(int direction)
    {
        //Return if player isn't grounded
        if (!_entity.isOnGround) return;

        //Get block in front of player acording to walk direction
        Block blockInFront = Location.LocationByPosition(
            (Vector2) transform.position + new Vector2(direction * 0.5f, 0)).GetBlock();

        if (blockInFront == null) return;
        
        if (Type.GetType(blockInFront.GetMaterial().ToString()).IsSubclassOf(typeof(Stairs)))
        {
            bool rotated_x = blockInFront.location.GetData().GetTag("rotated_x") == "true";
            bool rotated_y = blockInFront.location.GetData().GetTag("rotated_y") == "true";

            //if the stairs are rotated correctly
            if (rotated_y) return;
            if (direction == -1 && rotated_x) return;
            if (direction == 1 && !rotated_x) return;
            
            transform.position += new Vector3(direction * 0.2f, 1);
        }
    }
    
    [Server]
    private void WalkSound()
    {
        if (!_entity.isOnGround) return;
        if (Vector2.Distance(_lastStepPosition, transform.position) < StepSoundDistance) return;
        
        Sound.Play(_entity.Location, "entity/step", SoundType.Entities, 0.8f, 1.2f);
        _lastStepPosition = transform.position;
    }

    [Client]
    private void MovementParticleEffect()
    {
        if (!_entity.isOnGround) return;

        float walkParticleChance = 1.5f * Time.deltaTime * Mathf.Abs(_rb.velocity.x);
        System.Random r = new System.Random();

        if (r.NextDouble() > walkParticleChance) return;
        
        Block blockBeneath = (_entity.Location - new Location(0, 1)).GetBlock();
        if (blockBeneath == null) return;

        Particle part = Particle.ClientSpawn();
        Color[] textureColors = blockBeneath.GetColorsInTexture();

        part.transform.position = transform.position + new Vector3(0, 0.2f);
        part.color = textureColors[r.Next(textureColors.Length)];
        part.doGravity = true;
        part.velocity = new Vector2(
            _rb.velocity.x * -0.3f,
            Mathf.Abs(_rb.velocity.x) * 1.3f * (float)r.NextDouble());
        part.maxAge = .4f + (float)r.NextDouble() * .6f;
    }
    
    public virtual void LateUpdate()
    {
        _inLiquidLastFrame = _entity.isInLiquid;
    }
    
    private void Awake()
    {
        _entity = GetComponent<LivingEntity>();
        _rb = GetComponent<Rigidbody2D>();
        speed = walkSpeed;
    }
}
