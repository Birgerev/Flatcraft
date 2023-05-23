using System.Collections.Generic;
using Mirror;
using Unity.Mathematics;
using UnityEngine;
using Random = System.Random;

public class Enderman : Monster
{
    public override float maxHealth { get; } = 40;
    [SyncVar] public bool visuallyAngry;

    public override List<ItemStack> GetDrops()
    {
        //Drop a random amount of a certain item
        List<ItemStack> result = new();
        Random r = new(SeedGenerator.SeedByWorldLocation(Location));

        result.Add(new ItemStack(Material.Enderpearl, r.Next(0, 1 + 1)));

        return result;
    }
    
    public override void ClientUpdate()
    {
        base.ClientUpdate();

        //Spawn end particles every x frames
        if(Time.frameCount % 10 == 0)
            Particle.ClientSpawnVolume(
                Location.GetPosition() + new Vector2(-0.5f, -0.5f), 
                new Vector2(1.5f, 2.5f), new Vector2(0.8f, 0.8f), 
                new float2(0.6f, 1f), new int2(0, 3), new Color(.8f, .25f, .8f));
    }
    
    [Client]
    public override void UpdateAnimatorValues()
    {
        base.UpdateAnimatorValues();

        Animator anim = GetComponent<Animator>();

        anim.SetBool("angry", visuallyAngry);
    }

    public override EntityController GetController()
    {
        return new EndermanController(this);
    }
}