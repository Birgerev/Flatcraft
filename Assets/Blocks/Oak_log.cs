using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oak_Log : Block
{
    public override bool playerCollide { get; } = false;

    public static string default_texture = "block_oak_log";
    public override float breakTime { get; } = 3f;
    
    public override Tool_Type propperToolType { get; } = Tool_Type.Axe;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Wood;
}
