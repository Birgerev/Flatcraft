public class Dirt : Block
{
    public override string texture { get; set; } = "block_dirt";
    public override float breakTime { get; } = 0.75f;

    public override Tool_Type properToolType { get; } = Tool_Type.Shovel;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Dirt;
}