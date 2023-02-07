public class Red_Flower : Block
{
    public override string[] randomTextures { get; } =
        {"block_red_flower", "block_red_flower_1", "block_red_flower_2"};

    public override bool solid { get; set; } = false;
    public override float breakTime { get; } = 0.01f;
    public override bool requiresGround { get; } = true;
    public override bool isFlammable { get; } = true;

    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Grass;
}