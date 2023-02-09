
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vegetation : Block
{
    public override bool solid { get; set; } = false;
    public override float breakTime { get; } = 0.01f;
    public override bool isFlammable { get; } = true;

    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Grass;

    public override void Tick()
    {
        base.Tick();
        
        if(!ValidGround())
            Break();
    }

    public virtual bool ValidGround()
    {
        Material below = (location + new Location(0, -1)).GetMaterial();

        if (below == Material.Grass_Block)
            return true;

        return false;
    }
}
