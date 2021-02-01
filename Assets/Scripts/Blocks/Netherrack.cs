public class Netherrack : Block
{
    public override string texture { get; set; } = "block_netherrack";
    public override float breakTime { get; } = 2;

    public override Tool_Type propperToolType { get; } = Tool_Type.Pickaxe;
    public override Tool_Level propperToolLevel { get; } = Tool_Level.Wooden;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Stone;
}