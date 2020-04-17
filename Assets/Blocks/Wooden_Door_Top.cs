using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wooden_Door_Top : Door
{
    public override bool rotate_x { get; } = true;

    public override string open_texture { get; } = "block_wooden_door_top_open";
    public override string closed_texture { get; } = "block_wooden_door_top_close";

    public static string default_texture = "block_wooden_door_top_close";
    public override float breakTime { get; } = 3f;

    public override Tool_Type propperToolType { get; } = Tool_Type.Axe;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Wood;
}
