public class Bed_Bottom : Bed_Block
{
    public override BlockSoundType BlockSoundType { get; } = BlockSoundType.Wood;

    public override void BuildTick()
    {
        base.BuildTick();

        if (otherBlockLocation.GetMaterial() == Material.Air)
        {
            otherBlockLocation.SetMaterial(otherBlockMaterial).GetBlock().BuildTick();
            otherBlockLocation.Tick();
        }
        else
        {
            Break(true);
        }
    }
}