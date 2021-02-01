public class Bed_Top : Bed_Block
{
    public override string texture { get; set; } = "block_bed_top";

    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Wood;
}