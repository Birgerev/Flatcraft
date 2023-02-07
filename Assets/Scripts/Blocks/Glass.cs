public class Glass : Block
{
    public override bool solid { get; set; } = true;
    public override bool trigger { get; set; } = true;
    public override float breakTime { get; } = 0.45f;
    public override int glowLevel { get; } = 10;

    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Glass;
    
    public override ItemStack GetDrop()
    {
        return new ItemStack(Material.Air, 0);
    }
}