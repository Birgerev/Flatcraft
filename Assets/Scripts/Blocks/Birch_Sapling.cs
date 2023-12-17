using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Birch_Sapling : Block
{
    public override float AverageRandomTickDuration { get; } = 20 * 60;
    public override bool IsSolid { get; set; } = false;
    public override float BreakTime { get; } = 0.01f;
    public override bool IsFlammable { get; } = true;

    public override BlockSoundType BlockSoundType { get; } = BlockSoundType.Grass;
    
    public override void RandomTick()
    {
        base.RandomTick();
        
        location.SetState(new BlockState(Material.Structure_Block, new BlockData("structure=Birch_Tree"))).Tick();
    }
    
    public override void Tick()
    {
        base.Tick();
        Material matBelow = (location + new Location(0, -1)).GetMaterial();

        if (matBelow != Material.Grass_Block && matBelow != Material.Dirt)
        {
            Break();
        }
    }
}
