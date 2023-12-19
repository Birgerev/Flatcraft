using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Random = System.Random;

public class Wolf : PassiveEntity
{
    //Entity Properties
    public override float maxHealth { get; } = 8;
    protected virtual float tameChance { get; } = 0.3f;

    [SyncVar] public bool visuallyAngry;

    public override EntityController GetController()
    {
        return new WolfController(this);
    }
    
    [Server]
    public override void Interact(Player source)
    {
        base.Interact(source);
        
        PlayerInventory inv = source.GetInventoryHandler().GetInventory();
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

    [Server]
    public override void UpdateAnimatorValues()
    {
        base.UpdateAnimatorValues();

        Animator anim = GetComponent<Animator>();

        anim.SetBool("angry", visuallyAngry);
    }

    [ClientRpc]
    private void PlaySmokeEffect(Color color)
    {
        Particle.ClientSpawnSmallSmoke(transform.position + new Vector3(0, 2), color);
    }
}