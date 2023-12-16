public class Stone_Brick : Block
{
    public override float breakTime { get; } = 6;

    public override Tool_Type properToolType { get; } = Tool_Type.Pickaxe;
    public override Tool_Level properToolLevel { get; } = Tool_Level.Wooden;
    public override BlockSoundType blockSoundType { get; } = BlockSoundType.Stone;
}