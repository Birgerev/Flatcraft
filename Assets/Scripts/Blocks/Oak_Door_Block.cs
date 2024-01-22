public class Oak_Door_Block : Whole_Door
{
    public override float BreakTime { get; } = 3f;
    public override bool IsFlammable { get; } = true;

    public override Tool_Type ProperToolType { get; } = Tool_Type.Axe;
    public override BlockSoundType BlockSoundType { get; } = BlockSoundType.Wood;

    public override Material otherBlockMaterial => GetMaterial() == Material.Oak_Door_Bottom
        ? Material.Oak_Door_Top
        : Material.Oak_Door_Bottom;

    protected override ItemStack[] GetDrops()
    {
        return new[] { new ItemStack(Material.Oak_Door)};
    }
}