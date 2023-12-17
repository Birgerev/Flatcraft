public class Glass : Block
{
    public override bool Solid { get; set; } = false;
    public override float BreakTime { get; } = 0.45f;
    public override LightValues LightSourceValues { get; } = new LightValues(10);

    public override BlockSoundType BlockSoundType { get; } = BlockSoundType.Glass;

    protected override ItemStack[] GetDrops()
    {
        return null;
    }
}