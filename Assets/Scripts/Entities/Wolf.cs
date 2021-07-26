using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Wolf : PassiveEntity
{
    //Entity Properties
    public override float maxHealth { get; } = 8;
    protected override float walkSpeed { get; } = 6f;

    //TODO spawns
    public override EntityController GetController()
    {
        return new WolfController(this);
    }
    
    [Server]
    public override void Interact(Player source)
    {
        base.Interact(source);
        
        PlayerInventory inv = source.GetInventory();
        ItemStack heldItem = inv.GetSelectedItem();

        if (heldItem.material == Material.Bone)
        {
            heldItem.amount--;
            inv.SetItem(inv.selectedSlot, heldItem);
            Particle.Spawn_SmallSmoke(transform.position + new Vector3(0, 2), Color.red);
            
            Dog dog = (Dog)Entity.Spawn("Dog");
            dog.Teleport(Location);
            dog.ownerUuid = source.uuid;
            
            Remove();
        }
    }
}