public class Obsidian : Block
{
    public static string default_texture = "block_obsidian";
    public override float breakTime { get; } = 250;

    public override Tool_Type propperToolType { get; } = Tool_Type.Pickaxe;
    public override Tool_Level propperToolLevel { get; } = Tool_Level.Diamond;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Stone;
}