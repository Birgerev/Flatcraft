using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Dog : PassiveEntity
{
    //Entity Properties
    public override float maxHealth { get; } = 20;
    protected override float walkSpeed { get; } = 6f;
    [EntityDataTag(false)] public string ownerUuid;
    [EntityDataTag(false)] [SyncVar] public bool sitting;
    public SpriteRenderer collar;
    public override EntityController GetController()
    {
        return new DogController(this);
    }
    
    [Server]
    public Entity GetOwner()
    {
        return Entity.GetEntity(ownerUuid);
    }
    
    [Client]
    public override void UpdateAnimatorValues()
    {
        base.UpdateAnimatorValues();
        
        Animator anim = GetComponent<Animator>();

        anim.SetBool("sitting", sitting);
        anim.SetFloat("healthFraction", health / maxHealth);
    }
    
    [Server]
    public override void Interact(Player source)
    {
        base.Interact(source);
        
        if (source.uuid == ownerUuid)
        {
            sitting = !sitting;
        }
    }
}
