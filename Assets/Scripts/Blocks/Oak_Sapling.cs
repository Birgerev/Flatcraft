using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oak_Sapling : Block
{
    public override float averageRandomTickDuration { get; } = 20 * 60;
    public override bool solid { get; set; } = false;
    public override float breakTime { get; } = 0.01f;
    public override bool requiresGround { get; } = true;
    public override bool isFlammable { get; } = true;

    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Grass;
    
    public override void RandomTick()
    {
        base.RandomTick();
        
        location.SetState(new BlockState(Material.Structure_Block, new BlockData("structure=Oak_Tree"))).Tick();
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
