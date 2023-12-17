public class Gold_Ore : Block
{
    public override string[] RandomTextures { get; } = {"gold_ore", "gold_ore_1"};

    public override float BreakTime { get; } = 6;

    public override Tool_Type ProperToolType { get; } = Tool_Type.Pickaxe;
    public override Tool_Level ProperToolLevel { get; } = Tool_Level.Iron;
    public override BlockSoundType BlockSoundType { get; } = BlockSoundType.Stone;
}