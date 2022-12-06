public class Oak_Door_Block : Whole_Door
{
    public override float breakTime { get; } = 3f;
    public override bool isFlammable { get; } = true;

    public override Tool_Type properToolType { get; } = Tool_Type.Axe;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Wood;

    public override Material otherBlockMaterial => GetMaterial() == Material.Oak_Door_Bottom
        ? Material.Oak_Door_Top
        : Material.Oak_Door_Bottom;

    public override ItemStack GetDrop()
    {
        return new ItemStack(Material.Oak_Door, 1);
    }
}