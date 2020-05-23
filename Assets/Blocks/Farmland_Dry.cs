using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Farmland_Dry : Block
{
    public static string default_texture = "block_farmland_dry";
    public override float breakTime { get; } = 0.75f;

    public override Tool_Type propperToolType { get; } = Tool_Type.Shovel;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Dirt;
}
