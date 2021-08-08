using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Dog : PassiveEntity
{
    //Entity Properties
    private static List<Material> acceptableFoods { get; } = new List<Material>() 
    {Material.Raw_Beef, Material.Raw_Chicken, Material.Raw_Porkchop, Material.Steak, Material.Cooked_Chicken, 
        Material.Cooked_Porkchop, Material.Rotten_Flesh};
    protected virtual float foodHealthRegeneration { get; } = 20f / 3f;

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
            //Try feeding dog with owners held item
            if (TryFeed())
                return;
            
            //If feeding was not applicable, toggle sit state
            sitting = !sitting;
        }
    }

    private bool TryFeed()
    {
        Player owner = (Player) GetOwner();
        PlayerInventory inv = owner.GetInventory();
        if (inv == null)
            return false;
        ItemStack heldItem = inv.GetSelectedItem();

        if (!acceptableFoods.Contains(heldItem.material))
            return false;
        
        heldItem.amount--;
        inv.SetItem(inv.selectedSlot, heldItem);
        PlaySmokeEffect(Color.red);
        health = Mathf.Clamp(health + foodHealthRegeneration, 0, maxHealth); 

        return true;
    }
    
    [ClientRpc]
    private void PlaySmokeEffect(Color color)
    {
        Particle.Spawn_SmallSmoke(transform.position + new Vector3(0, 2), color);
    }
}
