public class Sandstone : Block
{
    public override string texture { get; set; } = "block_sandstone";
    public override float breakTime { get; } = 6;

    public override Tool_Type properToolType { get; } = Tool_Type.Pickaxe;
    public override Tool_Level properToolLevel { get; } = Tool_Level.Wooden;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Stone;
}