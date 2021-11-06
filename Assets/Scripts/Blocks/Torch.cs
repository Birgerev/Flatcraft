public class Torch : Block
{
    public override string texture { get; set; } = "block_torch_0";
    public override string[] alternativeTextures { get; } =
    {
        "block_torch_0", "block_torch_1", "block_torch_2", "block_torch_3"
    };
    public override float changeTextureTime { get; } = 0.3f;
    public override bool solid { get; set; } = false;
    public override float breakTime { get; } = 0.1f;

    public override int glowLevel { get; } = 15;

    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Wood;
}