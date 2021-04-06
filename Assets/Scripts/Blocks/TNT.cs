using UnityEngine;

public class TNT : Block
{
    public override string texture { get; set; } = "block_tnt";
    public override float breakTime { get; } = 0.1f;

    public override Tool_Type propperToolType { get; } = Tool_Type.None;
    public override Tool_Level propperToolLevel { get; } = Tool_Level.None;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Grass;

    public void Prime()
    {
        Prime(4);
    }
    
    public void Prime(float fuse)
    {
        location.SetMaterial(Material.Air).Tick();
        
        PrimedTNT tnt = (PrimedTNT)Entity.Spawn("PrimedTNT");
        tnt.Teleport(location);
        tnt.fuse = fuse;
    }
}