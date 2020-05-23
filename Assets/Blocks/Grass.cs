using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : Block
{
    public static string default_texture = "block_grass";
    public override float breakTime { get; } = 0.75f;
    
    public override bool autoTick { get; } = true;

    public override Tool_Type propperToolType { get; } = Tool_Type.Shovel;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Grass;

    public override ItemStack GetDrop()
    {
        return new ItemStack(Material.Dirt, 1);
    }
    
    
    public override void Tick(bool spread)
    {
        if (age <= 1)
        {
            TryDecay();
        }
        if (age > 1 && getRandomChance() < 0.05f)
        {
            TryDecay();
            TrySpread();
        }

        base.Tick(spread);
    }

    public void TryDecay()
    {
        if (Chunk.getBlock(location + new Location(0, 1)) != null)
        {
            //Turn to dirt if covered
            if (Chunk.getBlock(location + new Location(0, 1)).playerCollide)
            {
                Chunk.setBlock(location, Material.Dirt, "", false, false);
            }
        }
    }

    public void TrySpread()
    {
        System.Random r = new System.Random();
            
        Location loc = location + new Location((r.NextDouble() > 0.5f) ? 1 : -1, r.Next(-1, 1));
        
        Block block = Chunk.getBlock(loc);
        Block blockTop = Chunk.getBlock(loc + new Location(0, 1));
        if (block != null && block.GetMaterial() == Material.Dirt && (blockTop == null || !blockTop.playerCollide))
        {
            Chunk.setBlock(loc, Material.Grass, "", true, false);
        }
    }
}
