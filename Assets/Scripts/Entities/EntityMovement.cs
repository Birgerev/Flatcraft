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
    private const float StepSoundFrequencyMultiplier = 1.1f;
    
    [Header("Movement Properties")] 
    private const float AirDrag = 4.3f;
    private const float ClimbableFriction = 10f;
    private const float GroundFriction = 5f;
    private const float LiquidDrag = 10f;
    
    private const float Acceleration = 4f;
    private const float ClimbAcceleration = 55.0f;
    private const float SwimJumpVelocity = 2f;
    private const float SwimUpAcceleration = 45.0f;
    
    public float walkSpeed = 4.3f;
    
    [HideInInspector] public float speed;
    
    private float _lastJumpTime;
    private float _lastStepSoundTime;
    private bool _inLiquidLastFrame;
    private Vector2 _lastFramePosition;
    
    private LivingEntity _entity;

    private void Start()
    {
        _entity = GetComponent<LivingEntity>();
        speed = walkSpeed;
    }
    
    public virtual void LateUpdate()
    {
        _lastFramePosition = transform.position;
        _inLiquidLastFrame = _entity.isInLiquid;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isServer) return;//TODO call on server, for players too?
        
        WalkSoundCheck();
        ClientMovementParticleEffect();
        ApplyFriction();
        
        if (_inLiquidLastFrame && !_entity.isInLiquid && GetVelocity().y > 0)
            SetVelocity(GetVelocity() + new Vector2(0, SwimJumpVelocity));
    }
    
    [Server]
    public void WalkSoundCheck()
    {
        if (Mathf.Abs(_entity.GetVelocity().x) < 0.5f)
            return;
        
        float checkFrequency = StepSoundFrequencyMultiplier / Mathf.Abs(GetVelocity().x);
        float timeOffset = (float) new System.Random(_entity.uuid.GetHashCode()).NextDouble(); //Uses a static seed (id)

        if (Time.time - _lastStepSoundTime > checkFrequency && _entity.isOnGround)
        {
            WalkSound();
            _lastStepSoundTime = Time.time;
        }
    }

    public void ApplyFriction()
    {
        if (_entity.isInLiquid)
            SetVelocity(GetVelocity() * (1 / (1 + LiquidDrag * Time.deltaTime)));
        if (_entity.isOnClimbable)
            SetVelocity(GetVelocity() * (1 / (1 + ClimbableFriction * Time.deltaTime)));
        if (!_entity.isInLiquid && !_entity.isOnClimbable && !_entity.isOnGround)
            SetVelocity(new Vector3(GetVelocity().x * (1 / (1 + AirDrag * Time.deltaTime)), GetVelocity().y));
        if (!_entity.isInLiquid && !_entity.isOnClimbable && _entity.isOnGround)
            SetVelocity(GetVelocity() * (1 / (1 + GroundFriction * Time.deltaTime)));
    }

    public void Walk(int direction)
    {
        if (!isOwned && !isServer)
            return;

        if (GetVelocity().x < speed && GetVelocity().x > -speed)
        {
            float targetXVelocity = 0;

            if (direction == -1)
                targetXVelocity -= speed;
            else if (direction == 1)
                targetXVelocity += speed;
            else
                targetXVelocity = 0;

            GetComponent<Rigidbody2D>().velocity +=
                new Vector2(targetXVelocity * (Acceleration * Time.deltaTime), 0);
        }

        StairCheck(direction);
    }

    public void Jump()
    {
        if (!isOwned && !isServer)
            return;

        if (_entity.isOnGround && Time.time - _lastJumpTime >= 0.3f)
        {
            SetVelocity(new Vector2(GetVelocity().x, JumpVelocity));
            _lastJumpTime = Time.time;
        }

        if (_entity.isInLiquid)
            SetVelocity(GetVelocity() + new Vector2(0, SwimUpAcceleration * Time.deltaTime));

        if (_entity.isOnClimbable)
            SetVelocity(GetVelocity() + new Vector2(0, ClimbAcceleration * Time.deltaTime));
    }

    public void StairCheck(int direction)
    {
        if ((Vector2) transform.position != _lastFramePosition) //Return if player has moved since last frame
            return;
        if (!_entity.isOnGround) //Return if player isn't grounded
            return;

        //Get block in front of player acording to walk direction
        Block blockInFront = Location.LocationByPosition(
            (Vector2) transform.position + new Vector2(direction * 0.5f, 0)).GetBlock();

        if (blockInFront == null)
            return;
        
        if (Type.GetType(blockInFront.GetMaterial().ToString()).IsSubclassOf(typeof(Stairs)))
        {
            bool rotated_x = blockInFront.location.GetData().GetTag("rotated_x") == "true";
            bool rotated_y = blockInFront.location.GetData().GetTag("rotated_y") == "true";

            //if the stairs are rotated correctly
            if (rotated_y == false &&
                (direction == -1 && rotated_x == false || direction == 1 && rotated_x))
                transform.position += new Vector3(direction * 0.2f, 1);
        }
    }
    
    [Server]
    public virtual void WalkSound()
    {
        string soundName = "entity/step";
        Sound.Play(_entity.Location, soundName, SoundType.Entities, 0.8f, 1.2f);
    }
    
    [Client]
    protected void ClientMovementParticleEffect()
    {
        if (!_entity.isOnGround)
            return;

        float walkParticleChance = 1.5f;
        System.Random r = new System.Random();

        if (r.NextDouble() < walkParticleChance * Time.deltaTime * Mathf.Abs(GetVelocity().x))
        {
            Block blockBeneath = (_entity.Location - new Location(0, 1)).GetBlock();
            if (blockBeneath == null)
                return;

            Particle part = Particle.ClientSpawn();
            Color[] textureColors = blockBeneath.GetColorsInTexture();

            part.transform.position = transform.position + new Vector3(0, 0.2f);
            part.color = textureColors[r.Next(textureColors.Length)];
            part.doGravity = true;
            part.velocity = new Vector2(
                GetVelocity().x * -0.3f,
                Mathf.Abs(GetVelocity().x) * 1.3f *(float)r.NextDouble());
            part.maxAge = .4f + (float)r.NextDouble() * .6f;
        }
    }
    
    public virtual Vector2 GetVelocity()
    {
        return GetComponent<Rigidbody2D>().velocity;
    }
    
    public virtual void SetVelocity(Vector2 velocity)
    {
        GetComponent<Rigidbody2D>().velocity = velocity;
    }
}
