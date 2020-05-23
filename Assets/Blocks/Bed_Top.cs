using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bed_Top : Bed_Block
{
    public static string default_texture = "block_bed_top";

    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Wood;
}
