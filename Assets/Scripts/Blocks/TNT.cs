public class TNT : Block
{
    public override float BreakTime { get; } = 0.1f;

    public override Tool_Type ProperToolType { get; } = Tool_Type.None;
    public override Tool_Level ProperToolLevel { get; } = Tool_Level.None;
    public override BlockSoundType BlockSoundType { get; } = BlockSoundType.Grass;

    public void Prime()
    {
        Prime(4);
    }

    public void Prime(float fuse)
    {
        location.SetMaterial(Material.Air).Tick();

        PrimedTNT tnt = (PrimedTNT) Entity.Spawn("PrimedTNT");
        tnt.Teleport(location);
        tnt.fuse = fuse;
    }
}