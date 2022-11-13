public class End_Portal_Frame : Block
{
    public override string texture { get; set; } = "block_end_portal_frame";
    public override float breakTime { get; } = 100000000;

    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Stone;
}