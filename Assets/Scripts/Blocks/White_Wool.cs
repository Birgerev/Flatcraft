public class White_Wool : Block
{
    public override float breakTime { get; } = 1.25f;
    public override bool isFlammable { get; } = true;

    public override Tool_Type properToolType { get; } = Tool_Type.None;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Wool;
}