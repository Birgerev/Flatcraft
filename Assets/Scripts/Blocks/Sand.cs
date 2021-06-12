using UnityEngine;

public class Sand : Block
{
    public override string texture { get; set; } = "block_sand";
    public override float breakTime { get; } = 0.75f;

    public override Tool_Type propperToolType { get; } = Tool_Type.Shovel;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Sand;

    public override void Tick()
    {
        if ((location + new Location(0, -1)).GetMaterial() == Material.Air)
        {
            FallingBlock fs = (FallingBlock) Entity.Spawn("FallingBlock");
            fs.transform.position = location.GetPosition() - new Vector2(0, 0.5f);
            fs.material = GetMaterial();

            location.SetMaterial(Material.Air).Tick();
        }

        base.Tick();
    }
}