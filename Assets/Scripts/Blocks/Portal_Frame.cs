using UnityEngine;

public class Portal_Frame : Block
{
    public override bool solid { get; set; } = false;
    public override bool trigger { get; set; } = true;

    public override float breakTime { get; } = 9999999999f;
    public override LightValues lightSourceValues { get; } = new LightValues(11, new Color(1f, 0.47f, 0.92f), false);

    public override Tool_Type properToolType { get; } = Tool_Type.None;
    public override BlockSoundType blockSoundType { get; } = BlockSoundType.Glass;

    public override ItemStack[] GetDrops()
    {
        return null;
    }
}