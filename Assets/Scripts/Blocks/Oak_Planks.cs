public class Oak_Planks : Block
{
    public override float BreakTime { get; } = 3f;
    public override bool IsFlammable { get; } = true;

    public override Tool_Type ProperToolType { get; } = Tool_Type.Axe;
    public override BlockSoundType BlockSoundType { get; } = BlockSoundType.Wood;
}