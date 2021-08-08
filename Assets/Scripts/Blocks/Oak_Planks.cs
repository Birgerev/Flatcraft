public class Oak_Planks : Block
{
    public override string texture { get; set; } = "block_oak_planks";
    public override float breakTime { get; } = 3f;
    public override bool isFlammable { get; } = true;

    public override Tool_Type properToolType { get; } = Tool_Type.Axe;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Wood;
}