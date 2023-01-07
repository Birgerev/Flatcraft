using System;
using System.Collections;
using Mirror;
using UnityEngine;
using Random = System.Random;

public class LivingEntity : Entity
{
    //Entity Properties
    public static Color damageColor = new Color(1, 0.5f, 0.5f, 1);

    public Nameplate nameplate;

    //Entity Data Tags
    [EntityDataTag(false)] [SyncVar] public float health;
    [EntityDataTag(false)] [SyncVar] public string displayName;

    [Header("Movement Properties")] private readonly float acceleration = 4f;

    private readonly float airDrag = 4.3f;
    private readonly float climbableFriction = 10f;
    private readonly float climbAcceleration = 55.0f;
    private EntityController controller;
    private readonly float groundFriction = 5f;


    //Entity State
    protected float highestYlevelsinceground;
    protected bool inLiquidLastFrame;
    private readonly float jumpVelocity = 8.5f;
    private float lastJumpTime;
    private float lastStepSoundTime;
    private readonly float liquidDrag = 10f;
    protected float speed;
    private readonly float swimJumpVelocity = 2f;
    private readonly float swimUpAcceleration = 45.0f;
    protected virtual float walkSpeed { get; } = 4.3f;
    protected virtual float stepSoundFrequencyMultiplier { get; } = 1.1f;
    int checkDuration = 4;

    public virtual float maxHealth { get; } = 20;

    public override void Start()
    {
        base.Start();

        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
        speed = walkSpeed;
    }

    public override void Update()
    {
        base.Update();

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
        WalkSoundCheck();

        if (controller != null)
            controller.Tick();

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
        ClientMovementParticleEffect();
    }

    [Server]
    public void AmbientSoundCheck()
    {
        int checkDuration = 4;
        float timeOffset =
            (float) new Random(uuid.GetHashCode()).NextDouble() * checkDuration; //Uses a static seed (id)

        if ((Time.time + timeOffset) % checkDuration - Time.deltaTime <= 0)
            if (new Random(Time.time.GetHashCode() + uuid.GetHashCode()).NextDouble() < 0.5f)
                AmbientSound();
    }
    
    [Server]
    public void WalkSoundCheck()
    {
        if (Mathf.Abs(GetVelocity().x) < 0.5f)
            return;
        
        float checkFrequency = stepSoundFrequencyMultiplier / Mathf.Abs(GetVelocity().x);
        float timeOffset = (float) new Random(uuid.GetHashCode()).NextDouble(); //Uses a static seed (id)

        if (Time.time - lastStepSoundTime > checkFrequency && isOnGround)
        {
            WalkSound();
            lastStepSoundTime = Time.time;
        }
    }

    [Client]
    public virtual void UpdateAnimatorValues()
    {
        Animator anim = GetComponent<Animator>();

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
            Debug.LogWarning("Nameplate is missing for entity type: " + name);
            return;
        }

        nameplate.text = displayName;
    }

    public virtual void ProcessMovement()
    {
        if (inLiquidLastFrame && !isInLiquid && GetVelocity().y > 0)
            SetVelocity(GetVelocity() + new Vector2(0, swimJumpVelocity));

        ApplyFriction();
    }

    public void ApplyFriction()
    {
        if (isInLiquid)
            SetVelocity(GetVelocity() * (1 / (1 + liquidDrag * Time.deltaTime)));
        if (isOnClimbable)
            SetVelocity(GetVelocity() * (1 / (1 + climbableFriction * Time.deltaTime)));
        if (!isInLiquid && !isOnClimbable && !isOnGround)
            SetVelocity(new Vector3(GetVelocity().x * (1 / (1 + airDrag * Time.deltaTime)), GetVelocity().y));
        if (!isInLiquid && !isOnClimbable && isOnGround)
            SetVelocity(GetVelocity() * (1 / (1 + groundFriction * Time.deltaTime)));
    }

    public void Walk(int direction)
    {
        if (!hasAuthority && !isServer)
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
                new Vector2(targetXVelocity * (acceleration * Time.deltaTime), 0);
        }

        StairCheck(direction);
    }

    public void Jump()
    {
        if (!hasAuthority && !isServer)
            return;

        if (isOnGround && Time.time - lastJumpTime >= 0.3f)
        {
            SetVelocity(new Vector2(GetVelocity().x, jumpVelocity));
            lastJumpTime = Time.time;
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
    public virtual EntityController GetController()
    {
        return new EntityController(this);
    }
    
    [Server]
    private void FallDamageCheck()
    {
        if (isOnGround && !isInLiquid && age > 5)
        {
            float damage = highestYlevelsinceground - transform.position.y - 3;
            if (damage > 0)
            {
                Sound.Play(Location, "entity/land", SoundType.Entities, 0.5f, 1.5f); //Play entity land sound
                
                TakeFallDamage(damage);
                
                Block blockBelow = Location.LocationByPosition(transform.position - new Vector3(0, 0.2f))
                    .GetBlock();
                if(blockBelow != null)
                    FallDamageParticlesEffect(blockBelow.location);
                
            }
        }

        if (isOnGround || isInLiquid || isOnClimbable)
            highestYlevelsinceground = transform.position.y;
        else if (transform.position.y > highestYlevelsinceground)
            highestYlevelsinceground = transform.position.y;
    }
    
    [ClientRpc]
    private void FallDamageParticlesEffect(Location groundLocation)
    {
        Block blockBelow = groundLocation.GetBlock();
        Color[] textureColors = blockBelow.GetColorsInTexture();
        Random r = new Random();

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
    
    [Client]
    protected void ClientMovementParticleEffect()
    {
        if (!isOnGround)
            return;

        float walkParticleChance = 1.5f;
        Random r = new Random();

        if (r.NextDouble() < walkParticleChance * Time.deltaTime * Mathf.Abs(GetVelocity().x))
        {
            Block blockBeneath = (Location - new Location(0, 1)).GetBlock();
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
        DeathSmokeEffect();

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
        string soundName = "entity/" + GetType() + "/hurt";

        if (Sound.Exists(soundName))
            Sound.Play(Location, soundName, SoundType.Entities, 0.8f, 1.2f);
    }

    [Server]
    public virtual void DeathSound()
    {
        string soundName = "entity/" + GetType() + "/death";

        if (Sound.Exists(soundName))
            Sound.Play(Location, soundName, SoundType.Entities, 0.8f, 1.2f);
    }

    [Server]
    public virtual void AmbientSound()
    {
        string soundName = "entity/" + GetType() + "/idle";

        if (Sound.Exists(soundName))
            Sound.Play(Location, soundName, SoundType.Entities, 0.8f, 1.2f);
    }
    
    [Server]
    public virtual void WalkSound()
    {
        string soundName = "entity/step";
        Sound.Play(Location, soundName, SoundType.Entities, 0.8f, 1.2f);
    }

    public virtual void Knockback(Vector2 direction)
    {
        direction.Normalize();

        GetComponent<Rigidbody2D>().velocity += new Vector2(direction.x * 5f, 5f);
    }

    [ClientRpc]
    public void RedDamageEffect()
    {
        StartCoroutine(TurnRedByDamage());
    }

    [ClientRpc]
    public virtual void DeathSmokeEffect()
    {
        Particle.ClientSpawnSmallSmoke(transform.position, Color.white);
    }

    [ClientRpc]
    public virtual void DamageNumberEffect(int damage, Color color)
    {
        Particle.ClientSpawnNumber(transform.position + new Vector3(1, 2), damage, color);
    }
    
    private IEnumerator TurnRedByDamage()
    {
        Color baseColor = GetRenderer().color;

        for (int i = 0; i < 15; i++)
        {
            GetRenderer().color = damageColor;
            yield return new WaitForSeconds(0.01f);
        }

        GetRenderer().color = baseColor;
    }
}