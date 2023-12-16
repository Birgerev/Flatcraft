public class White_Wool : Block
{
    public override float breakTime { get; } = 1.25f;
    public override bool isFlammable { get; } = true;

    public override Tool_Type properToolType { get; } = Tool_Type.None;
    public override BlockSoundType blockSoundType { get; } = BlockSoundType.Wool;
}