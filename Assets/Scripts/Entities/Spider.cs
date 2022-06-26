using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Random = System.Random;

public class Spider : Monster
{
    public override float maxHealth { get; } = 16;

    public override List<ItemStack> GetDrops()
    {
        //Drop a random amount of a certain item
        List<ItemStack> result = new List<ItemStack>();
        Random r = new Random(SeedGenerator.SeedByLocation(Location));

        result.Add(new ItemStack(Material.Cobweb, r.Next(0, 1 + 1)));

        return result;
    }

    public override void Tick()
    {
        base.Tick();

        WallClimbCheck();
    }

    [Server]
    private void WallClimbCheck()
    {
        Block rightBlock = Location.LocationByPosition(transform.position + new Vector3((GetWidth()/2) + 0.5f, 0)).GetBlock();
        Block leftBlock = Location.LocationByPosition(transform.position + new Vector3(-((GetWidth()/2) + 0.5f), 0)).GetBlock();

        //Climb when there is a block in front of entity
        if ((rightBlock != null && rightBlock.solid) || (leftBlock != null && leftBlock.solid))
        {
            isOnClimbable = true;
            return;
        }
        
        isOnClimbable = false;
    }
    
    public override EntityController GetController()
    {
        return new SpiderController(this);
    }
}