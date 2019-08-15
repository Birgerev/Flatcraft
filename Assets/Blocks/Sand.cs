using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sand : Block
{
    public static string default_texture = "block_sand";
    public override float breakTime { get; } = 0.75f;

    public override void Tick()
    {
        base.Tick();

        if (Chunk.getBlock(getPosition() + new Vector2Int(0, -1)) == null)
        {
            FallingSand.Create(getPosition(), GetMateral());
            Chunk.setBlock(getPosition(), Material.Air, true);
        }
    }
}
