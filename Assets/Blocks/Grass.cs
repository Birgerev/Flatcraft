using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : Block
{
    public static string default_texture = "block_grass";
    public override float breakTime { get; } = 0.75f;
    public override float averageRandomTickDuration { get; } = 20;

    public override Tool_Type propperToolType { get; } = Tool_Type.Shovel;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Grass;

    public override ItemStack GetDrop()
    {
        return new ItemStack(Material.Dirt, 1);
    }

    public override void RandomTick()
    {
        TrySpread();
        TryDecay();
        
        base.RandomTick();
    }
    
    public override void Tick()
    {
        if (age == 0)
        {
            TryDecay();
        }

        base.Tick();
    }

    public void TryDecay()
    {
        Block blockAbove = (location + new Location(0, 1)).GetBlock();
        if (blockAbove != null)
        {
            //Turn to dirt if covered
            if (blockAbove.playerCollide)
            {
                location.SetMaterial(Material.Dirt);
            }
        }
    }

    public void TrySpread()
    {
        System.Random r = new System.Random();
            
        Location targetLoc = location + new Location((r.NextDouble() > 0.5f) ? 1 : -1, r.Next(-1, 1));
        Block blockAboveTarget = (targetLoc + new Location(0, 1)).GetBlock();
        if (targetLoc.GetMaterial() == Material.Dirt && (blockAboveTarget == null || !blockAboveTarget.playerCollide))
        {
            targetLoc.SetMaterial(Material.Grass);
        }
    }
}
