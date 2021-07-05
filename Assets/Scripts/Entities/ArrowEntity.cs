using Mirror;
using UnityEngine;

public class ArrowEntity : Projectile
{
    //Entity State
    //Entity Properties
    public override float triggerMargin { get; } = 0.5f;
    public override float entityDamage { get; } = 6;

    [Server]
    public override void Initialize()
    {
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;

        base.Initialize();
    }

    [Server]
    public override void Tick()
    {
        base.Tick();

        //Despawn
        if (age > 60 * 5)
            Die();

        GetComponent<Rigidbody2D>().simulated = !hasLanded; //TODO this makes box collider inactive, which in turn makes arrows not save
    }
    
    public override void HitEntity(Entity entity)
    {
        base.HitEntity(entity);
        
        Sound.Play(Location, "entity/arrow/hit", SoundType.Entities, 0.7f, 1.3f);
    }
    
    public override void HitBlock(Block block)
    {
        Sound.Play(Location, "entity/arrow/hit", SoundType.Entities, 0.7f, 1.3f);
    }
    
    [Client]
    public override void ClientUpdate()
    {
        RotateForwards();

        base.ClientUpdate();
    }
    
    private void RotateForwards()
    {
        Vector2 vel = GetComponent<Rigidbody2D>().velocity;

        if (vel == Vector2.zero)
            return;
        
        float angle = Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg;
        GetRenderer().transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}