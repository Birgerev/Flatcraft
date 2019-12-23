using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravel : Block
{
    public static string default_texture = "block_gravel";
    public override float breakTime { get; } = 0.75f;

    public override Tool_Type propperToolType { get; } = Tool_Type.Shovel;

    public override void Tick()
    {
        base.Tick();

        if (Chunk.getBlock(getPosition() + new Vector2Int(0, -1)) == null)
        {
            FallingSand fs = (FallingSand)Entity.Spawn("FallingSand");
            fs.transform.position = (Vector2)getPosition();
            fs.material = GetMaterial();

            Chunk.setBlock(getPosition(), Material.Air, true);
        }
    }
}
