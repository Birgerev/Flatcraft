public class Iron_Ore : Block
{
    public override string[] RandomTextures { get; } = {"iron_ore", "iron_ore_1"};

    public override float BreakTime { get; } = 6;

    public override Tool_Type ProperToolType { get; } = Tool_Type.Pickaxe;
    public override Tool_Level ProperToolLevel { get; } = Tool_Level.Stone;
    public override BlockSoundType BlockSoundType { get; } = BlockSoundType.Stone;
}