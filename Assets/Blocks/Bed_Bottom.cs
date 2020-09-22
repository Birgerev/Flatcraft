using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bed_Bottom : Bed_Block
{
    public static string default_texture = "block_bed_bottom";

    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Wood;

    public override void FirstTick()
    {
        base.FirstTick();

        Material otherMaterial = otherBlockLocation.GetMaterial();
        if (otherMaterial == Material.Air)
        {
            otherBlockLocation.SetMaterial(otherBlockMaterial);
        }
        else if (otherMaterial != otherBlockMaterial)
        {
            Break(true);
        }
    }
}
