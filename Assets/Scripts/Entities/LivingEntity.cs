using System;
using System.Collections;
using Mirror;
using UnityEngine;
using Random = System.Random;

public class LivingEntity : Entity
{
    //Entity Properties
    public static Color damageColor = new Color(1, 0.5f, 0.5f, 1);

    [Header("Movement Properties")] public float acceleration = 4f;

    public Nameplate nameplate;
    EntityController controller;

    //Entity Data Tags
    [EntityDataTag(false)] [SyncVar] public float health;
    [EntityDataTag(false)] [SyncVar] public string displayName;

    private float airDrag = 4.3f;
    private float climbableFriction = 10f;
    private float climbAcceleration = 55.0f;
    private float groundFriction = 5f;
    private float jumpVelocity = 8.5f;
    private float liquidDrag = 10f;
    private float sneakSpeed = 1.3f;
    private float sprintSpeed = 5.6f;
    private float swimUpAcceleration = 45.0f;
    private float swimJumpVelocity = 2f;
    private float walkSpeed = 4.3f;


    //Entity State
    public float highestYlevelsinceground;
    protected float last_jump_time;
    protected bool inLiquidLastFrame;
    [SyncVar]
    protected bool sprinting;
    [SyncVar]
    protected bool sneaking;
    public virtual float maxHealth { get; } = 20;

    public override void Start()
    {
        base.Start();
        
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public override void Update()
    {
        base.Update();

        CalculateFlip();
        inLiquidLastFrame = isInLiquid;
    }

    [Server]
    public override void Initialize()
    {
        health = maxHealth;
        controller = GetController();
        
        base.Initialize();
    }
    
    [Server]
    public override void Tick()
    {
        base.Tick();
        
        AmbientSoundCheck();
        
        if(controller != null)
            controller.Tick();
        
        
        //Sprinting particles    
        if (Mathf.Abs(GetVelocity().x) >= sneakSpeed && isOnGround)
        {
            float chances;
            if (sprinting)
                chances = 0.2f;
            else
                chances = 0.02f;

            MovementParticlesEffect(chances);
        }
        
        ProcessMovement();
        FallDamageCheck();
    }

    [Server]
    public override void Teleport(Location loc)
    {
        highestYlevelsinceground = 0;
        base.Teleport(loc);
    }
    
    [Client]
    public override void ClientUpdate()
    {
        base.ClientUpdate();

        UpdateAnimatorValues();
        UpdateNameplate();
    }

    [Server]
    public void AmbientSoundCheck()
    {
        int checkDuration = 4;
        float timeOffset = (float)new System.Random(uuid.GetHashCode()).NextDouble() * checkDuration;    //Uses a static seed (id)
        
        if (((Time.time + timeOffset) % checkDuration) - Time.deltaTime <= 0)
            if(new System.Random(Time.time.GetHashCode() + uuid.GetHashCode()).NextDouble() < 0.5f)
                AmbientSound();
    }
    
    [Client]
    public virtual void UpdateAnimatorValues()
    {
        var anim = GetComponent<Animator>();

        if (anim == null)
            return;

        if (anim.isInitialized)
            anim.SetFloat("velocity-x", Mathf.Abs(GetVelocity().x));
    }
    
    [Client]
    public virtual void UpdateNameplate()
    {
        if (nameplate == null)
        {
            Debug.LogWarning("Nameplate is missing for entity type: " + this.name);
            return;
        }
        
        nameplate.text = displayName;
    }

    public virtual void ProcessMovement()
    {
        if(inLiquidLastFrame && !isInLiquid && GetVelocity().y > 0)
            SetVelocity(GetVelocity() + new Vector2(0, swimJumpVelocity));
        
        ApplyFriction();
        CrouchOnLadderCheck();
    }

    public void ApplyFriction()
    {
        if (isInLiquid)
            SetVelocity(GetVelocity() * (1 / (1 + (liquidDrag * Time.deltaTime))));
        if (isOnClimbable)
            SetVelocity(GetVelocity() * (1 / (1 + (climbableFriction * Time.deltaTime))));
        if (!isInLiquid && !isOnClimbable && !isOnGround)
            SetVelocity(new Vector3(GetVelocity().x * (1 / (1 + (airDrag * Time.deltaTime))), GetVelocity().y));
        if (!isInLiquid && !isOnClimbable && isOnGround)
            SetVelocity(GetVelocity() * (1 / (1 + (groundFriction * Time.deltaTime))));
    }

    public void Walk(int direction)
    {
        if (!hasAuthority)
            return;
        
        float maxSpeed;
        if (sprinting)
            maxSpeed = sprintSpeed;
        else if (sneaking)
            maxSpeed = sneakSpeed;
        else
            maxSpeed = walkSpeed;

        if (GetVelocity().x < maxSpeed && GetVelocity().x > -maxSpeed)
        {
            float targetXVelocity = 0;

            if (direction == -1)
                targetXVelocity -= maxSpeed;
            else if (direction == 1)
                targetXVelocity += maxSpeed;
            else targetXVelocity = 0;

            GetComponent<Rigidbody2D>().velocity +=
                new Vector2(targetXVelocity * (acceleration * Time.deltaTime), 0);
        }

        StairCheck(direction);
    }

    public void Jump()
    { 
        if (!hasAuthority)
            return;

        if (isOnGround && Time.time - last_jump_time >= 0.3f)
        {
            SetVelocity(new Vector2(GetVelocity().x, jumpVelocity));
            last_jump_time = Time.time;
        }

        if (isInLiquid) 
            SetVelocity(GetVelocity() + new Vector2(0, swimUpAcceleration * Time.deltaTime));

        if (isOnClimbable) 
            SetVelocity(GetVelocity() + new Vector2(0, climbAcceleration * Time.deltaTime));
    }

    public void StairCheck(int direction)
    {
        if ((Vector2) transform.position != lastFramePosition) //Return if player has moved since last frame
            return;
        if (!isOnGround) //Return if player isn't grounded
            return;
        
        //Get block in front of player acording to walk direction
        Block blockInFront = Location.LocationByPosition(
                (Vector2) transform.position + new Vector2(direction * 0.5f, -0.5f)).GetBlock(); 

        if (blockInFront == null) return;

        if (Type.GetType(blockInFront.GetMaterial().ToString()).IsSubclassOf(typeof(Stairs)))
        {
            bool rotated_x = blockInFront.location.GetData().GetTag("rotated_x") == "true";
            bool rotated_y = blockInFront.location.GetData().GetTag("rotated_y") == "true";
            
            //if the stairs are rotated correctly
            if (rotated_y == false && 
                ((direction == -1 && rotated_x == false) || (direction == 1 && rotated_x)))
                transform.position += new Vector3(direction * 0.2f, 1);
        }
    }

    [Server]
    public virtual EntityController GetController()
    {
        return new EntityController(this);
    }

    public void CalculateFlip()
    {
        if (Mathf.Abs(GetVelocity().x) > 0.1f)
            facingLeft = GetVelocity().x < 0;
    }

    public void CrouchOnLadderCheck()
    {
        if (isOnClimbable && sneaking)
        {
            SetVelocity(new Vector2(GetVelocity().x, 0.45f));        //y should be 0, but 0.45 prevents any downwards movement
        }
    }

    [Server]
    private void FallDamageCheck()
    {
        if (isOnGround && !isInLiquid)
        {
            var damage = highestYlevelsinceground - transform.position.y - 3;
            if (damage >= 1)
            {
                Sound.Play(Location, "entity/land", SoundType.Entities, 0.5f, 1.5f); //Play entity land sound

                FallDamageParticlesEffect();

                TakeFallDamage(damage);
            }
        }

        if (isOnGround || isInLiquid || isOnClimbable)
            highestYlevelsinceground = transform.position.y;
        else if (transform.position.y > highestYlevelsinceground)
            highestYlevelsinceground = transform.position.y;
    }

    [ClientRpc]
    private void FallDamageParticlesEffect()
    {
        var r = new Random();
        Block blockBeneath = null;
        for (var y = -1; blockBeneath == null && y > -3; y--)
        {
            var block = (Location + new Location(0, y)).GetBlock();
            if (block != null)
                blockBeneath = block;
        }

        if (blockBeneath == null)
            return;
        
        var particleAmount = r.Next(4, 8);
        for (var i = 0; i < particleAmount; i++) //Spawn landing partickes
        {
            Particle part = Particle.Spawn();

            part.transform.position = blockBeneath.location.GetPosition() + new Vector2(0, 0.6f);
            part.color = blockBeneath.GetRandomColourFromTexture();
            part.doGravity = true;
            part.velocity = new Vector2(((float) r.NextDouble() - 0.5f) * 2, 1.5f);
            part.maxAge = 1f + (float) r.NextDouble();
            part.maxBounces = 10;
        }
    }

    [ClientRpc]
    private void MovementParticlesEffect(float chances)
    {
        var r = new Random();

        if (r.NextDouble() < chances)
        {
            Block blockBeneath = null;
            for (var y = 1; blockBeneath == null && y < 3; y++)
            {
                var block = (Location - new Location(0, y)).GetBlock();
                if (block != null && block.solid)
                    blockBeneath = block;
            }

            if (blockBeneath == null)
                return;

            Particle part = Particle.Spawn();

            part.transform.position = blockBeneath.location.GetPosition() + new Vector2(0, 0.6f);
            part.color = blockBeneath.GetRandomColourFromTexture();
            part.doGravity = true;
            part.velocity = -(GetVelocity() * 0.2f);
            part.maxAge = (float) r.NextDouble();
            part.maxBounces = 10;
        }
    }

    [Server]
    public override void Hit(float damage, Entity source)
    {
        base.Hit(damage, source);

        Knockback(transform.position - source.transform.position);
    }

    [Server]
    public virtual void TakeFallDamage(float damage)
    {
        Damage(damage);
    }

    [Server]
    public override void Damage(float damage)
    {
        HurtSound();
        DamageSound();

        health -= damage;

        if (health <= 0)
            Die();

        RedDamageEffect();
    }

    [Server]
    public override void Die()
    {
        if (dead)
            return;
        
        DeathSound();
        Particle.Spawn_SmallSmoke(transform.position, Color.white);

        base.Die();
    }

    [Server]
    public void DamageSound()
    {
        Sound.Play(Location, "entity/damage", SoundType.Entities, 0.5f, 1.5f);
    }

    [Server]
    public virtual void HurtSound()
    {
        string soundName = "entity/" + this.GetType().ToString() + "/hurt";
        
        if(Sound.Exists(soundName))
            Sound.Play(Location, soundName, SoundType.Entities, 0.8f, 1.2f);
    }

    [Server]
    public virtual void DeathSound()
    {
        string soundName = "entity/" + this.GetType().ToString() + "/death";
        
        if(Sound.Exists(soundName))
            Sound.Play(Location, soundName, SoundType.Entities, 0.8f, 1.2f);
    }
    
    [Server]
    public virtual void AmbientSound()
    {
        string soundName = "entity/" + this.GetType().ToString() + "/idle";
        
        if(Sound.Exists(soundName))
            Sound.Play(Location, soundName, SoundType.Entities, 0.8f, 1.2f);
    }

    public virtual void Knockback(Vector2 direction)
    {
        direction.Normalize();

        GetComponent<Rigidbody2D>().velocity += new Vector2(direction.x * 3f, 4f);
    }

    [ClientRpc]
    public void RedDamageEffect()
    {
        StartCoroutine(TurnRedByDamage());
    }
    
    private IEnumerator TurnRedByDamage()
    {
        var baseColor = GetRenderer().color;

        for (int i = 0; i < 15; i++)
        {
            GetRenderer().color = damageColor;
            yield return new WaitForSeconds(0.01f);
        }
        GetRenderer().color = baseColor;
    }
}