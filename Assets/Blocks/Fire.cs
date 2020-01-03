using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : Block
{
    public static string default_texture = "block_fire_0";
    public override string[] alternative_textures { get; } = { "block_fire_0", "block_fire_1", "block_fire_2" };
    public override float change_texture_time { get; } = 1;

    public override bool playerCollide { get; } = false;
    public override float breakTime { get; } = 0.01f;

    public override ItemStack GetDrop()
    {
        return new ItemStack();
    }

    public override void Tick(bool spread)
    {
        if (getRandomChance() < 0.1f / Chunk.TickRate)
        {
            bool spreadFire = (100 / Chunk.TickRate < 50 / Chunk.TickRate);

            if (spreadFire)
            {
                int r = Random.Range(0, 2);

                if (r == 0 && Chunk.getBlock(position + new Vector2Int(-1, 0)) == null && Chunk.getBlock(position + new Vector2Int(-1, -1)) != null)
                {
                    Chunk.setBlock(position + new Vector2Int(-1, 0), Material.Fire);
                }
                if (r == 1 && Chunk.getBlock(position + new Vector2Int(1, 0)) == null && Chunk.getBlock(position + new Vector2Int(1, -1)) != null)
                {
                    Chunk.setBlock(position + new Vector2Int(1, 0), Material.Fire);
                }
                if (r == 2 && Chunk.getBlock(position + new Vector2Int(0, 2)) == null && Chunk.getBlock(position + new Vector2Int(0, 1)) != null)
                {
                    Chunk.setBlock(position + new Vector2Int(0, 2), Material.Fire);
                }
            }
            else
            {
                Chunk.setBlock(position, Material.Air);
            }
        }

        base.Tick(spread);
    }
}
