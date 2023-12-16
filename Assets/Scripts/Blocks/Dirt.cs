public class Dirt : Block
{
    public override float breakTime { get; } = 0.75f;

    public override Tool_Type properToolType { get; } = Tool_Type.Shovel;
    public override BlockSoundType blockSoundType { get; } = BlockSoundType.Dirt;
}