
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vegetation : Block
{
    public override bool IsSolid { get; set; } = false;
    public override float BreakTime { get; } = 0.01f;
    public override bool IsFlammable { get; } = true;

    public override BlockSoundType BlockSoundType { get; } = BlockSoundType.Grass;

    public override void Tick()
    {
        base.Tick();
        
        if(!GroundCheck())
            Break();
    }

    private bool GroundCheck()
    {
        Material belowMat = (location + new Location(0, -1)).GetMaterial();

        return ValidGround().Contains(belowMat);
    }
    
    protected virtual List<Material> ValidGround()
    {
        return new List<Material> { Material.Grass_Block};
    }
}
