using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Projectile : Entity
{
    public bool hasLanded = false;
    //Entity Data Tags
    [EntityDataTag(false)] public string ownerUuid;
    public virtual float triggerMargin { get; } = 0;
    public virtual float entityDamage { get; } = 0;
    
    [Server]
    public override void Tick()
    {
        base.Tick();
        
        CheckTrigger();
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
        if (hitBlock != null && !hasLanded)
            HitBlock(hitBlock);

        if (hitEntity != null && hitBlock == null)
            HitEntity(hitEntity);
        
        hasLanded = hitBlock;
    }
    
    public virtual void HitEntity(Entity entity)
    {
        entity.Damage(entityDamage);
        entity.lastDamager = Entity.GetEntity(ownerUuid);
        this.Remove();
    }
    
    public virtual void HitBlock(Block block)
    {
        this.Remove();
    }
    
    private int GetCollisionMask()
    {
        return Physics2D.GetLayerCollisionMask(gameObject.layer);
    }

    public override void TakeSuffocationDamage(float damage)
    {
        //Disable suffocation damage
    }
    
    [Server]
    public override void Hit(float damage, Entity source)
    {
        //Disabling Hit
    }
}
