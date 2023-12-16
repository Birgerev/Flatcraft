public class Dead_Bush : Block
{
    public override string[] randomTextures { get; } =
        {"dead_bush", "dead_bush_1", "dead_bush_2"};

    public override bool solid { get; set; } = false;
    public override float breakTime { get; } = 0.01f;
    public override bool requiresGround { get; } = true;
    public override bool isFlammable { get; } = true;

    public override BlockSoundType blockSoundType { get; } = BlockSoundType.Grass;

    protected override ItemStack[] GetDrops()
    {
        return new[] { new ItemStack(Material.Stick)};
    }
}