public class Dirt : Block
{
    public override float BreakTime { get; } = 0.75f;

    public override Tool_Type ProperToolType { get; } = Tool_Type.Shovel;
    public override BlockSoundType BlockSoundType { get; } = BlockSoundType.Dirt;
}