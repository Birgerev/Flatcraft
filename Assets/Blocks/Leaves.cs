using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leaves : Block
{
    public static string default_texture = "block_leaves";
    public override bool playerCollide { get; } = false;
    public override float breakTime { get; } = 0.3f;

    public override ItemStack GetDrop()
    {
        return new ItemStack();
    }

    public override void Tick(bool spread)
    {
        if (randomTickNumber < 100 / Chunk.TickRate)
            TryDecay();

        base.Tick(spread);
    }

    public void TryDecay()
    {
        int range = 4;
        bool foundSupport = false;

        for (int x = -range; x < range; x++)
        {
            for (int y = -range; y < range; y++)
            {
                if (Chunk.getBlock(new Vector2Int(getPosition().x + x, getPosition().y + y)) != null)
                {
                    if (Chunk.getBlock(new Vector2Int(getPosition().x + x, getPosition().y + y)).GetMaterial() == Material.Oak_Log)
                    {
                        foundSupport = true;
                        break;
                    }
                }
            }
        }

        if (!foundSupport)
        {
            Break();
        }
    }
}
