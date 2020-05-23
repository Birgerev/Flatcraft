using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Farmland_Wet : Block
{
    public static string default_texture = "block_farmland_wet";
    public override float breakTime { get; } = 0.75f;

    public override Tool_Type propperToolType { get; } = Tool_Type.Shovel;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Dirt;
    
    public override ItemStack GetDrop()
    {
        return new ItemStack(Material.Dirt, 1);
    }
}
