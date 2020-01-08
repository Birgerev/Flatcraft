using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bed_Bottom : Bed
{
    public static string default_texture = "block_bed_bottom";
        
    public override void FirstTick()
    {
        base.FirstTick();

        Block otherBlock = Chunk.getBlock(otherBlockPosition);
        if (otherBlock == null)
        {
            otherBlock = Chunk.setBlock(otherBlockPosition, otherBlockMaterial, "", true, true);
        }
        else if (otherBlock.GetMaterial() != otherBlockMaterial)
        {
            Break(true);
        }
    }
}
