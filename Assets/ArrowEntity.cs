using Mirror;
using UnityEngine;

public class ArrowEntity : Entity
{
    //Entity State
    //Entity Properties
    public bool stuckOnBlock = false;
    //Entity Data Tags
    public float triggerMargin;

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

        CheckTrigger();
        GetComponent<Rigidbody2D>().simulated = !stuckOnBlock; //TODO this makes box collider inactive, which in turn makes arrows not save
    }

    [Server]
    private void CheckTrigger()
    {
        if (dead || age < 0.2f)
            return;
        
        Vector2 position = (Vector2)transform.position + GetComponent<BoxCollider2D>().offset;
        Vector2 triggerSize = GetComponent<BoxCollider2D>().size + new Vector2(triggerMargin, triggerMargin);
        Collider2D[] cols = Physics2D.OverlapBoxAll(position, triggerSize, 0, GetCollisionMask());
        
        Entity hitEntity = null;
        Block hitBlock = null;
        foreach (Collider2D col in cols)
        {
            if(col.gameObject == gameObject)
                continue;

            if (col.GetComponent<Block>() != null)
                hitBlock = col.GetComponent<Block>();
            
            if (col.GetComponent<Entity>() != null)
                hitEntity = col.GetComponent<Entity>();
        }
        
        //If just hit block, play hit sound
        if (!stuckOnBlock && hitBlock)
            Sound.Play(Location, "entity/arrow/hit", SoundType.Entities, 0.7f, 1.3f);
        
        stuckOnBlock = hitBlock;

        if (hitEntity != null && !stuckOnBlock)
        {
            HitEntity(hitEntity);
        }
    }
    
    private void HitEntity(Entity entity)
    {
        Sound.Play(Location, "entity/arrow/hit", SoundType.Entities, 0.7f, 1.3f);
        entity.Damage(6);
        this.Die();
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
    
    private int GetCollisionMask()
    {
        return Physics2D.GetLayerCollisionMask(gameObject.layer);
    }

    public override void TakeSuffocationDamage(float damage)
    {
        //Disable suffocation damage and float upwards instead
        Teleport(Location + new Location(0, 1));
    }


    [Server]
    public override void Hit(float damage, Entity source)
    {
        //Disabling Hit
    }
}