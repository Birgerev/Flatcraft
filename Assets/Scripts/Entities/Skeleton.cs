using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Random = System.Random;

public class Skeleton : Monster
{
    public override float maxHealth { get; } = 20;
    protected override bool burnUnderSun { get; } = true;
    private float arrowSpeed = 18;
    public float bowInaccuracy = 0.5f;

    public bool isShooting = false;
    
    public override List<ItemStack> GetDrops()
    {
        //Drop a random amount of a certain item
        List<ItemStack> result = new List<ItemStack>();
        Random r = new Random(SeedGenerator.SeedByLocation(Location));

        result.Add(new ItemStack(Material.Bone, r.Next(0, 2 + 1)));

        return result;
    }

    [Client]
    public override void UpdateAnimatorValues()
    {
        base.UpdateAnimatorValues();

        Animator anim = GetComponent<Animator>();

        anim.SetBool("shooting", isShooting);
    }

    [Server]
    public void AimAndShootEntity(Entity target)
    {
        StartCoroutine(ShootEntity(target));
    }

    IEnumerator ShootEntity(Entity target)
    {
        isShooting = true;
        yield return new WaitForSeconds(1.6f);
        
        if (target != null)
        {
            Vector2 velocity = ProjectileArcCalculator.CalculateProjectileVelocityForTarget(
                GetShootPosition() , (Vector2)target.transform.position + new Vector2(0, 1), arrowSpeed, target.GetVelocity());

            velocity *= (1 - (bowInaccuracy/2) + ((float)new Random().NextDouble() * bowInaccuracy));
        
            Shoot(velocity);
        }

        isShooting = false;
    }
        

    [Server]
    public void Shoot(Vector2 velocity)
    {
        ArrowEntity arrow = (ArrowEntity) Entity.Spawn("ArrowEntity");
        arrow.Teleport(Location.LocationByPosition(GetShootPosition()));
        arrow.GetComponent<Rigidbody2D>().velocity = velocity;
        arrow.ownerUuid = uuid;
        
        Sound.Play(Location, "random/bow_shoot", SoundType.Entities, 0.7f, 1.3f);
    }
    
    private Vector2 GetShootPosition()
    {
        return (Vector2) transform.position + new Vector2((facingLeft) ? -1 : 1, 1);
    }
    
    public override EntityController GetController()
    {
        return new SkeletonController(this);
    }
}