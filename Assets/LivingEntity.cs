using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : Entity
{
    //Entity Properties
    public static Color damageColor = new Color(1, 0.5f, 0.5f, 1);
    public virtual float maxHealth { get; } = 20;
    
    [Header("Movement Properties")]
    private float acceleration = 4f;
    private float walkSpeed = 4.3f;
    private float sprintSpeed = 5.6f;
    private float sneakSpeed = 1.3f;
    private float swimUpSpeed = 2f;
    private float climbSpeed = 0.35f;
    private float jumpVelocity = 8f;
    private float groundFriction = 0.92f;
    private float airDrag = 0.92f;
    private float liquidDrag = 0.75f;
    private float ladderFriction = 0.95f;

    //Entity Data Tags
    [EntityDataTag(false)]
    public float health;
    

    //Entity State
    protected bool sprinting;
    protected bool sneaking;
    protected float last_jump_time;
    protected float highestYlevelsinceground;
    protected EntityController controller;

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

        controller.Tick();
        CalculateFlip();
        UpdateAnimatorValues();

        if (Mathf.Abs(getVelocity().x) >= sneakSpeed && isOnGround)
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
        Animator anim = GetComponent<Animator>();

        if (anim == null)
            return;

        if(anim.isInitialized)
            anim.SetFloat("velocity-x", Mathf.Abs(getVelocity().x));
    }

    public virtual void ProcessMovement()
    {
        ApplyFriction();
    }

    public virtual void ApplyFriction()
    {
        if (isInLiquid)
            setVelocity(getVelocity() * liquidDrag);
        if (isOnLadder)
            setVelocity(getVelocity() * ladderFriction);
        if (!isInLiquid && !isOnLadder && !isOnGround)
            setVelocity(new Vector3(getVelocity().x * airDrag, getVelocity().y));
        if (!isInLiquid && !isOnLadder && isOnGround)
            setVelocity(getVelocity() * groundFriction);
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
        
        if (getVelocity().x < maxSpeed && getVelocity().x > -maxSpeed)
        {
            float targetXVelocity = 0;

            if (direction == -1)
                targetXVelocity -= maxSpeed;
            else if (direction == 1)
                targetXVelocity += maxSpeed;
            else targetXVelocity = 0;
            
            GetComponent<Rigidbody2D>().velocity += new Vector2(targetXVelocity * (acceleration * Time.fixedDeltaTime), 0);
        }

        StairCheck(direction);
    }

    public void StairCheck(int direction)
    {
        if ((Vector2)transform.position != lastFramePosition)    //Return if player has moved since last frame
            return;
        if (!isOnGround)    //Return if player isn't grounded
            return;
        
        Block blockInFront = Location.LocationByPosition((Vector2)transform.position + new Vector2(direction*0.7f, -0.5f), location.dimension).GetBlock();    //Get block in front of player acording to walk direction

        if (blockInFront == null)
        {
            return;
        }
        
        if (System.Type.GetType(blockInFront.GetMaterial().ToString()).IsSubclassOf(typeof(Stairs)))
        {
            bool rotated_x = false;
            bool rotated_y = false;

            rotated_x = (blockInFront.data.GetData("rotated_x") == "true");
            rotated_y = (blockInFront.data.GetData("rotated_y") == "true");
            
            if (rotated_y == false && ((direction == -1 && rotated_x == false) || (direction == 1 &&  rotated_x == true)))    //if the stairs are rotated correctly
            {
                transform.position += new Vector3(0, 1);
            }
        }
    }

    public virtual EntityController GetController()
    {
        return new EntityController(this);
    }

    public virtual void CalculateFlip()
    {
        if (getVelocity().x != 0)
        {
            facingLeft = (getVelocity().x < 0);
        }
    }

    public virtual void Jump()
    {
        if (isOnGround)
        {
            if (Time.time - last_jump_time < 0.7f)
                return;

            setVelocity(getVelocity() + new Vector2(0, jumpVelocity));
            last_jump_time = Time.time;
        }

        if (isInLiquid && getVelocity().y < swimUpSpeed)
        {
            setVelocity(getVelocity() + new Vector2(0, swimUpSpeed));
        }
        
        if (isOnLadder)
        {
            setVelocity(getVelocity() + new Vector2(0, climbSpeed));
        }
    }


    private void fallDamageCheck()
    {
        if (isOnGround && !isInLiquid)
        {
            float damage = (highestYlevelsinceground - transform.position.y) - 3;
            if (damage >= 1)
            {
                Sound.Play(location, "entity/land", SoundType.Entities, 0.5f, 1.5f);    //Play entity land sound

                spawnFallDamageParticles();

                TakeFallDamage(damage);
            }
        }

        if (isOnGround || isInLiquid || isOnLadder)
            highestYlevelsinceground = transform.position.y;
        else if (transform.position.y > highestYlevelsinceground)
            highestYlevelsinceground = transform.position.y;
    }

    private void spawnFallDamageParticles()
    {
        System.Random r = new System.Random();
        Block blockBeneath = null;
        for (int y = -1; blockBeneath == null && y > -3; y--)
        {
            Block block = (location + new Location(0, y)).GetBlock();
            if (block != null)
                blockBeneath = block;
        }
                
        int particleAmount = r.Next(4, 8);
        for (int i = 0; i < particleAmount; i++)    //Spawn landing partickes
        {
            Particle part = (Particle)Entity.Spawn("Particle");

            part.transform.position = blockBeneath.location.GetPosition() + new Vector2(0,  0.6f);
            part.color = blockBeneath.GetRandomColourFromTexture();
            part.doGravity = true;
            part.velocity = new Vector2(((float)r.NextDouble() - 0.5f) * 2, 1.5f);
            part.maxAge = 1f + (float)r.NextDouble();
            part.maxBounces = 10;
        }
    }
    
    private void spawnMovementParticles(float chances)
    {
        System.Random r = new System.Random();

        if (r.NextDouble() < chances)
        {
            Block blockBeneath = null;
            for (int y = 1; blockBeneath == null && y < 3; y++)
            {
                Block block = (location - new Location(0, y)).GetBlock();
                if (block != null && block.playerCollide)
                    blockBeneath = block;
            }


            Particle part = (Particle) Entity.Spawn("Particle");

            part.transform.position = blockBeneath.location.GetPosition() + new Vector2(0, 0.6f);
            part.color = blockBeneath.GetRandomColourFromTexture();
            part.doGravity = true;
            part.velocity = -(getVelocity() * 0.2f);
            part.maxAge = (float) r.NextDouble();
            part.maxBounces = 10;
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
        Sound.Play(location, "entity/damage", SoundType.Entities, 0.5f, 1.5f);

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

        GetComponent<Rigidbody2D>().velocity += new Vector2(direction.x*3f, 4f);
    }

    IEnumerator TurnRedByDamage()
    {
        Color baseColor = getRenderer().color;

        getRenderer().color = damageColor;
        yield return new WaitForSeconds(0.15f);
        getRenderer().color = baseColor;
    }
}
