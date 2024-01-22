using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class EntityMovement : NetworkBehaviour
{
    private const float JumpVelocity = 9f;
    private const float StepSoundDistance = .9f;
    
    [Header("Movement Properties")] 
    private const float AirDrag = 4.3f;
    private const float ClimbableFriction = 10f;
    private const float GroundFriction = 7f;
    private const float LiquidDrag = 10f;
    
    private const float Acceleration = 7f;
    private const float ClimbAcceleration = 67.0f;
    private const float ExitLiquidJumpVelocity = 2.5f;
    private const float SwimUpAcceleration = 55.0f;
    
    public float walkSpeed = 4.3f;
    
    [HideInInspector] public float speed;
    
    private Vector2 _lastStepPosition;
    private bool _inLiquidLastFrame;
    
    private LivingEntity _entity;
    private Rigidbody2D _rb;


    // Update is called once per frame
    void Update()
    {
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
        
        _rb.velocity = new Vector2(_rb.velocity.x, JumpVelocity);
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
