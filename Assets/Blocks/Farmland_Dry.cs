using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Farmland_Dry : Block
{
    public static string default_texture = "block_farmland_dry";
    public override float breakTime { get; } = 0.75f;
    public override bool autoTick { get; } = true;

    public override Tool_Type propperToolType { get; } = Tool_Type.Shovel;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Dirt;
    
    public override ItemStack GetDrop()
    {
        return new ItemStack(Material.Dirt, 1);
    }
    
    public override void Tick(bool spreadTick)
    {
        if (getRandomChance() < 0.2f / Chunk.TickRate)
            CheckWater();
        
        base.Tick(spreadTick);
    }

    public void CheckWater()
    {
        bool hasWater = false;
        for (int x = -4; x <= 4; x++)
        {
            Block block = Chunk.getBlock(location + new Location(x, 0));

            if (block != null && block.GetMaterial() == Material.Water)
            {
                hasWater = true;
                break;
            }
        }

        if (!hasWater)
            DryUp();
        if (hasWater)
            BecomeWet();
    }

    public void DryUp()
    {
        Chunk.setBlock(location, Material.Dirt);
    }
    
    public void BecomeWet()
    {
        Chunk.setBlock(location, Material.Farmland_Wet);
    }
}
