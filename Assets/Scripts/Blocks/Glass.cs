public class Glass : Block
{
    public override string texture { get; set; } = "block_glass";
    public override bool solid { get; set; } = true;
    public override bool trigger { get; set; } = true;
    public override float breakTime { get; } = 0.45f;
    public override int glowLevel { get; } = 10;

    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Glass;
}