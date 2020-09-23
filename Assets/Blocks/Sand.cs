using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sand : Block
{
    public static string default_texture = "block_sand";
    public override float breakTime { get; } = 0.75f;

    public override Tool_Type propperToolType { get; } = Tool_Type.Shovel;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Sand;

    public override void Tick()
    {
        if ((location + new Location(0, -1)).GetMaterial() == Material.Air)
        {
            FallingSand fs = (FallingSand)Entity.Spawn("FallingSand");
            fs.transform.position = location.GetPosition();
            fs.material = GetMaterial();

            location.SetMaterial(Material.Air);
        }

        base.Tick();
    }
}
