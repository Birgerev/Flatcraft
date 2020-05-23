using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bed : Item
{
    public static string default_texture = "item_bed";
    
    public override void InteractRight(Location loc, bool firstFrameDown)
    {
        Block block = Chunk.getBlock(loc);

        if (block == null)
        {
            Chunk.setBlock(loc, Material.Bed_Bottom);
        }
        
        base.InteractRight(loc, firstFrameDown);
    }
}
