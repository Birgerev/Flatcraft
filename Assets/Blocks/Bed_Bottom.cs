using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bed_Bottom : Bed
{
    public static string default_texture = "block_bed_bottom";

    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Wood;

    public override void FirstTick()
    {
        base.FirstTick();

        Block otherBlock = Chunk.getBlock(otherBlockLocation);
        if (otherBlock == null)
        {
            otherBlock = Chunk.setBlock(otherBlockLocation, otherBlockMaterial, "", true, true);
        }
        else if (otherBlock.GetMaterial() != otherBlockMaterial)
        {
            Break(true);
        }
    }
}
