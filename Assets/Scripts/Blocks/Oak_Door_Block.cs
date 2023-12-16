public class Oak_Door_Block : Whole_Door
{
    public override float breakTime { get; } = 3f;
    public override bool isFlammable { get; } = true;

    public override Tool_Type properToolType { get; } = Tool_Type.Axe;
    public override BlockSoundType blockSoundType { get; } = BlockSoundType.Wood;

    public override Material otherBlockMaterial => GetMaterial() == Material.Oak_Door_Bottom
        ? Material.Oak_Door_Top
        : Material.Oak_Door_Bottom;

    public override ItemStack[] GetDrops()
    {
        return new[] { new ItemStack(Material.Oak_Door)};
    }
}