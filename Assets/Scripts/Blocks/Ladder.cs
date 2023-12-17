public class Ladder : Block
{
    public override bool Solid { get; set; } = false;
    public override bool IsClimbable { get; } = true;

    public override float BreakTime { get; } = 3f;
    public override bool IsFlammable { get; } = true;

    public override Tool_Type ProperToolType { get; } = Tool_Type.Axe;
    public override BlockSoundType BlockSoundType { get; } = BlockSoundType.Ladder;
}