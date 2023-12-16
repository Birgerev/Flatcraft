public class Oak_Plank_Stairs : Stairs
{
    public override float breakTime { get; } = 3f;
    public override bool isFlammable { get; } = true;

    public override Tool_Type properToolType { get; } = Tool_Type.Axe;
    public override BlockSoundType blockSoundType { get; } = BlockSoundType.Wood;
}