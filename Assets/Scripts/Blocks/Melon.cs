using System;

public class Melon : Block
{
    public override float breakTime { get; } = 3f;
    public override bool isFlammable { get; } = true;
    public override bool requiresGround { get; } = true;

    public override Tool_Type properToolType { get; } = Tool_Type.Axe;
    public override BlockSoundType blockSoundType { get; } = BlockSoundType.Wood;

    protected override ItemStack[] GetDrops()
    {
        return new[] { new ItemStack(Material.Melon_Slice, new Random().Next(3, 7))};
    }
}