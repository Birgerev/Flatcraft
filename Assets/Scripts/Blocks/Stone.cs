public class Stone : Block
{
    public override float BreakTime { get; } = 6;

    public override Tool_Type ProperToolType { get; } = Tool_Type.Pickaxe;
    public override Tool_Level ProperToolLevel { get; } = Tool_Level.Wooden;
    public override BlockSoundType BlockSoundType { get; } = BlockSoundType.Stone;

    protected override ItemStack[] GetDrops()
    {
        return new[] { new ItemStack(Material.Cobblestone)};
    }
}