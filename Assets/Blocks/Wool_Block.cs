public class Wool_Block : Block
{
    public override string texture { get; set; } = "block_wool_block";
    public override float breakTime { get; } = 1.25f;
    public override bool isFlammable { get; } = true;

    public override Tool_Type propperToolType { get; } = Tool_Type.None;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Wool;
}