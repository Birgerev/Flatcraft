using System;

public class Melon : Block
{
    public override float BreakTime { get; } = 3f;
    public override bool IsFlammable { get; } = true;
    public override bool RequiresGround { get; } = true;

    public override Tool_Type ProperToolType { get; } = Tool_Type.Axe;
    public override BlockSoundType BlockSoundType { get; } = BlockSoundType.Wood;

    protected override ItemStack[] GetDrops()
    {
        return new[] { new ItemStack(Material.Melon_Slice, new Random().Next(3, 7))};
    }
}