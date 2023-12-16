public class Dead_Bush : Block
{
    public override string[] RandomTextures { get; } =
        {"dead_bush", "dead_bush_1", "dead_bush_2"};

    public override bool Solid { get; set; } = false;
    public override float BreakTime { get; } = 0.01f;
    public override bool RequiresGround { get; } = true;
    public override bool IsFlammable { get; } = true;

    public override BlockSoundType BlockSoundType { get; } = BlockSoundType.Grass;

    protected override ItemStack[] GetDrops()
    {
        return new[] { new ItemStack(Material.Stick)};
    }
}