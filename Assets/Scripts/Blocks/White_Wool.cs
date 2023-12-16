public class White_Wool : Block
{
    public override float BreakTime { get; } = 1.25f;
    public override bool IsFlammable { get; } = true;

    public override Tool_Type ProperToolType { get; } = Tool_Type.None;
    public override BlockSoundType BlockSoundType { get; } = BlockSoundType.Wool;
}