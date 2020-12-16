using UnityEngine;

public class Wooden_Door : Whole_Door
{
    public override float breakTime { get; } = 3f;
    public override bool isFlammable { get; } = true;

    public override Tool_Type propperToolType { get; } = Tool_Type.Axe;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Wood;
    public override Material otherBlockMaterial => GetMaterial() == Material.Wooden_Door_Bottom
        ? Material.Wooden_Door_Top
        : Material.Wooden_Door_Bottom;
}