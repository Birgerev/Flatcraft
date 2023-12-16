public class Glass : Block
{
    public override bool solid { get; set; } = true;
    public override bool trigger { get; set; } = true;
    public override float breakTime { get; } = 0.45f;
    public override LightValues lightSourceValues { get; } = new LightValues(10);

    public override BlockSoundType blockSoundType { get; } = BlockSoundType.Glass;
    
    public override ItemStack[] GetDrops()
    {
        return null;
    }
}