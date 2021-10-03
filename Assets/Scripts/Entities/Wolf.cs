using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Random = System.Random;

public class Wolf : PassiveEntity
{
    //Entity Properties
    public override float maxHealth { get; } = 8;
    protected override float walkSpeed { get; } = 6f;
    protected virtual float tameChance { get; } = 0.3f;

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
            heldItem.Amount--;
            inv.SetItem(inv.selectedSlot, heldItem);
            if (new Random().NextDouble() < tameChance)
            {
                PlaySmokeEffect(Color.red);

                Dog dog = (Dog) Entity.Spawn("Dog");
                dog.Teleport(Location);
                dog.ownerUuid = source.uuid;

                Remove();
            }
            else
            {
                PlaySmokeEffect(Color.black);
            }
        }
    }
    
    [ClientRpc]
    private void PlaySmokeEffect(Color color)
    {
        Particle.Spawn_SmallSmoke(transform.position + new Vector3(0, 2), color);
    }
}