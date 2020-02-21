using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sand : Block
{
    public static string default_texture = "block_sand";
    public override float breakTime { get; } = 0.75f;

    public override Tool_Type propperToolType { get; } = Tool_Type.Shovel;

    public override void Tick(bool spread)
    {
        if (Chunk.getBlock(location + new Location(0, -1)) == null)
        {
            FallingSand fs = (FallingSand)Entity.Spawn("FallingSand");
            fs.transform.position = location.getPosition();
            fs.material = GetMaterial();

            Chunk.setBlock(location, Material.Air, true);
        }

        base.Tick(spread);
    }
}
