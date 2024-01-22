public class Redstone_Ore : Block
{
    public override string[] RandomTextures { get; } = {"redstone_ore", "redstone_ore_1"};

    public override float BreakTime { get; } = 6;

    public override Tool_Type ProperToolType { get; } = Tool_Type.Pickaxe;
    public override Tool_Level ProperToolLevel { get; } = Tool_Level.Iron;
    public override BlockSoundType BlockSoundType { get; } = BlockSoundType.Stone;

    protected override ItemStack[] GetDrops()
    {
        return new[] { new ItemStack(Material.Redstone_Dust)};//TODO random amount
    }
}