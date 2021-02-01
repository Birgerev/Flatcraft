public class Iron_Ore : Block
{
    public override string texture { get; set; } = "block_iron_ore_0";
    public override string[] alternative_textures { get; } = {"block_iron_ore_0", "block_iron_ore_1"};

    public override float breakTime { get; } = 6;

    public override Tool_Type propperToolType { get; } = Tool_Type.Pickaxe;
    public override Tool_Level propperToolLevel { get; } = Tool_Level.Stone;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Stone;
}