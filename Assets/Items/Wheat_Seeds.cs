using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheat_Seeds : Item
{
    public static string default_texture = "item_wheat_seeds";
    
    public override void InteractRight(Location loc, bool firstFrameDown)
    {
        Block block = Chunk.getBlock(loc);

        if (block == null)
        {
            Chunk.setBlock(loc, Material.Wheat_Crop);
        }
        
        base.InteractRight(loc, firstFrameDown);
    }
}
