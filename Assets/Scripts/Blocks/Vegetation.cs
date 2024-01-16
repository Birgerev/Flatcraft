
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vegetation : Block
{
    public override bool IsSolid { get; set; } = false;
    public override float BreakTime { get; } = 0.01f;
    public override bool IsFlammable { get; } = true;

    public override BlockSoundType BlockSoundType { get; } = BlockSoundType.Grass;

    public override bool CanExistAt(Location loc)
    {
        Material belowMat = (location + new Location(0, -1)).GetMaterial();

        if (!ValidGround().Contains(belowMat)) return false;

        return base.CanExistAt(loc);
    }
    
    protected virtual List<Material> ValidGround()
    {
        return new List<Material> { Material.Grass_Block};
    }
}
