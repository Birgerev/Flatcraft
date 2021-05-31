using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Random = System.Random;

public class Creeper : Monster
{
    private float maxFuse = 1.5f;
    [SyncVar] 
    public float fuse;
    [SyncVar] 
    public bool ignited;
    public override float maxHealth { get; } = 20;

    public override List<ItemStack> GetDrops()
    {
        //Drop a random amount of a certain item
        List<ItemStack> result = new List<ItemStack>();
        Random r = new Random(SeedGenerator.SeedByLocation(Location));

        result.Add(new ItemStack(Material.Gunpowder, r.Next(0, 2 + 1)));

        return result;
    }

    [Server]
    public override void Tick()
    {
        base.Tick();

        if (ignited)
        {
            fuse += Time.deltaTime;
        }

        if (fuse >= maxFuse)
            Explode();
    }

    [Server]
    public void SetIgnited(bool ignited)
    {
        this.ignited = ignited;

        if(!ignited)
            fuse = 0;
    }

    [Server]
    public void Explode()
    {
        Explosion.Create(Location, 4, 1);
        Die();
    }
    
    [Client]
    public override void UpdateAnimatorValues()
    {
        base.UpdateAnimatorValues();
        
        Animator anim = GetComponent<Animator>();
        
        anim.SetBool("primed", ignited);
    }
    
    public override EntityController GetController()
    {
        return new CreeperController(this);
    }
}
