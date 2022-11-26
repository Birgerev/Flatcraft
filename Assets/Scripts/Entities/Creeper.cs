using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Random = System.Random;

public class Creeper : Monster
{
    [SyncVar] public float fuse;

    [SyncVar] public bool ignited;

    private readonly float maxFuse = 1.5f;
    public override float maxHealth { get; } = 20;
    private bool dropDisc = false;

    public override List<ItemStack> GetDrops()
    {
        //Drop a random amount of a certain item
        List<ItemStack> result = new List<ItemStack>();
        Random r = new Random(SeedGenerator.SeedByWorldLocation(Location));

        result.Add(new ItemStack(Material.Gunpowder, r.Next(0, 2 + 1)));
        if(dropDisc)
            result.Add(new ItemStack(Material.Music_Disc_Stal, 1));

        return result;
    }

    [Server]
    public override void Tick()
    {
        base.Tick();

        if (ignited)
            fuse += Time.deltaTime;

        if (fuse >= maxFuse)
            Explode();
    }

    [Server]
    public void SetIgnited(bool ignited)
    {
        this.ignited = ignited;

        if (ignited)
            Sound.Play(Location, "entity/Creeper/fuse", SoundType.Entities, 0.8f, 1.2f);
        else
            fuse = 0;
    }

    [Server]
    public void Explode()
    {
        Explosion.Create(Location, 3, 1);
        Remove();
    }

    [Client]
    public override void UpdateAnimatorValues()
    {
        base.UpdateAnimatorValues();

        Animator anim = GetComponent<Animator>();

        anim.SetBool("primed", ignited);
    }

    [Server]
    public override void Die()
    {
        if (dead)
            return;

        if (lastDamager is Skeleton)
        {
            dropDisc = true;
        }
        
        base.Die();
    }

    public override EntityController GetController()
    {
        return new CreeperController(this);
    }
}