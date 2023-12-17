public class Cobweb : Block
{
    public override float BreakTime { get; } = 10f;
    public override bool IsSolid { get; set; } = false;

    public override Tool_Type ProperToolType { get; } = Tool_Type.Sword;
    public override BlockSoundType BlockSoundType { get; } = BlockSoundType.Stone;
}