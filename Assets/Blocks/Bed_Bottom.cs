public class Bed_Bottom : Bed_Block
{
    public static string default_texture = "block_bed_bottom";

    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Wood;

    public override void BuildTick()
    {
        base.BuildTick();

        if (otherBlockLocation.GetMaterial() == Material.Air)
        {
            otherBlockLocation.SetMaterial(otherBlockMaterial).GetBlock().BuildTick();
            otherBlockLocation.Tick();
        }
        else 
            Break(true);
    }
}