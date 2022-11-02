using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.Mathematics;
using UnityEngine;

public class EnderEyeEntity :  Entity
{
    //TODO private const
    
    [Server]
    public override void Spawn()
    {
        base.Spawn();

        
    }

    [Server]
    public override void Tick()
    {
        base.Tick();

    }
    
    public override List<ItemStack> GetDrops()
    {
        //TODO random return
        List<ItemStack> result = new List<ItemStack>();
        result.Add(new ItemStack(Material.Eye_Of_Ender, 1));

        return result;
    }

    public override void TakeSuffocationDamage(float damage) { }//Disable suffocation damage
    
    [Server]
    public override void Hit(float damage, Entity source) { }//Disabling Hit
}
