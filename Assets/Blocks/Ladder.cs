using UnityEngine;

public class Ladder : Block
{
    public static string default_texture = "block_ladder";
    public override bool solid { get; set; } = false;
    public override bool trigger { get; set; } = true;
    public override bool climbable { get; } = true;

    public override float breakTime { get; } = 3f;
    public override bool isFlammable { get; } = true;

    public override Tool_Type propperToolType { get; } = Tool_Type.Axe;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Wood;
}