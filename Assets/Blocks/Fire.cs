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

    public override void Tick()
    {
        if (getRandomChance() < 0.1f / Chunk.TickRate)
        {
            System.Random random = new System.Random();
            Location spreadLocation = new Location();

            while (spreadLocation.Equals(new Location()))
            {
                int x = random.Next(-1, 1);
                int y = random.Next(-1, 1);
                
                if((location + new Location(x, y)).GetMaterial() == Material.Air && (location + new Location(x, y - 1)).GetMaterial() != Material.Air)
                    (location + new Location(x, y)).SetMaterial(Material.Fire);
            }
        }
        if (getRandomChance() < 0.1f / Chunk.TickRate)
        {
            location.SetMaterial(Material.Air);
        }

        base.Tick();
    }
}
