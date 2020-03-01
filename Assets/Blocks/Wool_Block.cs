using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wool_Block : Block
{
    public static string default_texture = "block_wool_block";
    public override float breakTime { get; } = 1.25f;

    public override Tool_Type propperToolType { get; } = Tool_Type.None;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Wool;

}
