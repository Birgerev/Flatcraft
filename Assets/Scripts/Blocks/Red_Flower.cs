public class Red_Flower : Block
{
    public override string texture { get; set; } = "block_red_flower_0";

    public override string[] alternativeTextures { get; } =
        {"block_red_flower_0", "block_red_flower_1", "block_red_flower_2"};

    public override bool solid { get; set; } = false;
    public override float breakTime { get; } = 0.01f;
    public override bool requiresGround { get; } = true;
    public override bool isFlammable { get; } = true;

    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Grass;
}