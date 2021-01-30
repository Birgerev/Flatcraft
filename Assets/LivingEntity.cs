using System;
using System.Collections;
using UnityEngine;
using Random = System.Random;

public class LivingEntity : Entity
{
    //Entity Properties
    public static Color damageColor = new Color(1, 0.5f, 0.5f, 1);

    [Header("Movement Properties")] private readonly float acceleration = 4f;

    private readonly float airDrag = 0.92f;
    private readonly float climbSpeed = 1.2f;
    protected EntityController controller;
    private readonly float groundFriction = 0.92f;

    //Entity Data Tags
    [EntityDataTag(false)] public float health;

    private readonly float jumpVelocity = 8f;
    private readonly float ladderFriction = 0.8f;
    private readonly float liquidDrag = 0.75f;
    private readonly float sneakSpeed = 1.3f;
    private readonly float sprintSpeed = 5.6f;
    private readonly float swimUpSpeed = 2f;
    private readonly float walkSpeed = 4.3f;


    //Entity State
    public float highestYlevelsinceground;
    protected float last_jump_time;
    protected bool sprinting;
    protected bool sneaking;
    public virtual float maxHealth { get; } = 20;

    public override void Start()
    {
        health = maxHealth;

        base.Start();

        controller = GetController();

        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public virtual void FixedUpdate()
    {
        ProcessMovement();
        fallDamageCheck();
    }

    public override void Update()
    {
        base.Update();
        
        if(controller != null)
            controller.Tick();
        CalculateFlip();
        UpdateAnimatorValues();

        if (Mathf.Abs(GetVelocity().x) >= sneakSpeed && isOnGround)
        {
            float chances;
            if (sprinting)
                chances = 0.2f;
            else
                chances = 0.02f;

            spawnMovementParticles(chances);
        }
    }

    public virtual void UpdateAnimatorValues()
    {
        var anim = GetComponent<Animator>();

        if (anim == null)
            return;

        if (anim.isInitialized)
            anim.SetFloat("velocity-x", Mathf.Abs(GetVelocity().x));
    }

    public virtual void ProcessMovement()
    {
        ApplyFriction();
        CrouchOnLadder();
    }

    public virtual void ApplyFriction()
    {
        if (isInLiquid)
            SetVelocity(GetVelocity() * liquidDrag);
        if (isOnClimbable)
            SetVelocity(GetVelocity() * ladderFriction);
        if (!isInLiquid && !isOnClimbable && !isOnGround)
            SetVelocity(new Vector3(GetVelocity().x * airDrag, GetVelocity().y));
        if (!isInLiquid && !isOnClimbable && isOnGround)
            SetVelocity(GetVelocity() * groundFriction);
    }

    public virtual void Walk(int direction)
    {
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
                new Vector2(targetXVelocity * (acceleration * Time.fixedDeltaTime), 0);
        }

        StairCheck(direction);
    }

    public void StairCheck(int direction)
    {
        if ((Vector2) transform.position != lastFramePosition) //Return if player has moved since last frame
            return;
        if (!isOnGround) //Return if player isn't grounded
            return;

        var blockInFront = Location
            .LocationByPosition((Vector2) transform.position + new Vector2(direction * 0.7f, -0.5f), Location.dimension)
            .GetBlock(); //Get block in front of player acording to walk direction

        if (blockInFront == null) return;

        if (Type.GetType(blockInFront.GetMaterial().ToString()).IsSubclassOf(typeof(Stairs)))
        {
            var rotated_x = false;
            var rotated_y = false;

            rotated_x = blockInFront.data.GetData("rotated_x") == "true";
            rotated_y = blockInFront.data.GetData("rotated_y") == "true";

            if (rotated_y == false && (direction == -1 && rotated_x == false || direction == 1 && rotated_x)
            ) //if the stairs are rotated correctly
                transform.position += new Vector3(0, 1);
        }
    }

    public virtual EntityController GetController()
    {
        return new EntityController(this);
    }

    public virtual void CalculateFlip()
    {
        if (GetVelocity().x != 0) facingLeft = GetVelocity().x < 0;
    }

    public virtual void Jump()
    {
        if (isOnGround)
        {
            if (Time.time - last_jump_time < 0.3f)
                return;

            SetVelocity(GetVelocity() + new Vector2(0, jumpVelocity));
            last_jump_time = Time.time;
        }

        if (isInLiquid && GetVelocity().y < swimUpSpeed) SetVelocity(GetVelocity() + new Vector2(0, swimUpSpeed));

        if (isOnClimbable) SetVelocity(GetVelocity() + new Vector2(0, climbSpeed));
    }

    public virtual void CrouchOnLadder()
    {
        if (isOnClimbable && sneaking)
        {
            SetVelocity(new Vector2(GetVelocity().x, 0.45f));        //y should be 0, but 0.45 prevents any downwards movement
        }
    }

    private void fallDamageCheck()
    {
        if (isOnGround && !isInLiquid)
        {
            var damage = highestYlevelsinceground - transform.position.y - 3;
            if (damage >= 1)
            {
                Sound.Play(Location, "entity/land", SoundType.Entities, 0.5f, 1.5f); //Play entity land sound

                spawnFallDamageParticles();

                TakeFallDamage(damage);
            }
        }

        if (isOnGround || isInLiquid || isOnClimbable)
            highestYlevelsinceground = transform.position.y;
        else if (transform.position.y > highestYlevelsinceground)
            highestYlevelsinceground = transform.position.y;
    }

    private void spawnFallDamageParticles()
    {
        var r = new Random();
        Block blockBeneath = null;
        for (var y = -1; blockBeneath == null && y > -3; y--)
        {
            var block = (Location + new Location(0, y)).GetBlock();
            if (block != null)
                blockBeneath = block;
        }

        var particleAmount = r.Next(4, 8);
        for (var i = 0; i < particleAmount; i++) //Spawn landing partickes
        {
            var part = (Particle) Spawn("Particle");

            part.transform.position = blockBeneath.location.GetPosition() + new Vector2(0, 0.6f);
            part.color = blockBeneath.GetRandomColourFromTexture();
            part.doGravity = true;
            part.velocity = new Vector2(((float) r.NextDouble() - 0.5f) * 2, 1.5f);
            part.maxAge = 1f + (float) r.NextDouble();
            part.maxBounces = 10;
        }
    }

    private void spawnMovementParticles(float chances)
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

            var particle = (Particle) Spawn("Particle");

            particle.transform.position = blockBeneath.location.GetPosition() + new Vector2(0, 0.6f);
            particle.color = blockBeneath.GetRandomColourFromTexture();
            particle.doGravity = true;
            particle.velocity = -(GetVelocity() * 0.2f);
            particle.maxAge = (float) r.NextDouble();
            particle.maxBounces = 10;
        }
    }

    public override void Hit(float damage)
    {
        base.Hit(damage);

        Knockback(transform.position - Player.localInstance.transform.position);
    }

    public virtual void TakeFallDamage(float damage)
    {
        Damage(damage);
    }

    public override void Damage(float damage)
    {
        Sound.Play(Location, "entity/damage", SoundType.Entities, 0.5f, 1.5f);

        health -= damage;

        if (health <= 0)
            Die();

        StartCoroutine(TurnRedByDamage());
    }

    public override void Die()
    {
        Particle.Spawn_SmallSmoke(transform.position, Color.white);

        base.Die();
    }

    public virtual void Knockback(Vector2 direction)
    {
        direction.Normalize();

        GetComponent<Rigidbody2D>().velocity += new Vector2(direction.x * 3f, 4f);
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