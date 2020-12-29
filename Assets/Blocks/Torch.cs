public class Torch : Block
{
    public override string texture { get; set; } = "block_torch";
    public override bool solid { get; set; } = false;
    public override float breakTime { get; } = 0.1f;
    public override bool requiresGround { get; } = true;

    public override int glowLevel { get; } = 15;

    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Wood;
}