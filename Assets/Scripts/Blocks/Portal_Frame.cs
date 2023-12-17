using UnityEngine;

public class Portal_Frame : Block
{
    public override bool Solid { get; set; } = false;

    public override float BreakTime { get; } = 9999999999f;
    public override LightValues LightSourceValues { get; } = new LightValues(11, new Color(1f, 0.47f, 0.92f), false);

    public override Tool_Type ProperToolType { get; } = Tool_Type.None;
    public override BlockSoundType BlockSoundType { get; } = BlockSoundType.Glass;

    protected override ItemStack[] GetDrops()
    {
        return null;
    }
}