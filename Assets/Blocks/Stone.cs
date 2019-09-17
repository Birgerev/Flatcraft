using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone : Block
{
    public static string default_texture = "block_stone";
    public override float breakTime { get; } = 6;

    public override Tool_Type propperToolType { get; } = Tool_Type.Pickaxe;
    public override Tool_Level propperToolLevel { get; } = Tool_Level.Wooden;

    public override void GeneratingTick()
    {
        base.GeneratingTick();
        
        System.Random r = new System.Random(Chunk.seedByPosition(getPosition()));

        //Generate Ores
        if (r.NextDouble() < Chunk.ore_diamond_chance && getPosition().y <= Chunk.ore_diamond_height)
        {
            Chunk.setBlock(getPosition(), Material.Structure_Block, "structure=Ore_Diamond|save=false", false);
            return;
        }
        if (r.NextDouble() < Chunk.ore_redstone_chance && getPosition().y <= Chunk.ore_redstone_height)
        {
            Chunk.setBlock(getPosition(), Material.Structure_Block, "structure=Ore_Redstone|save=false", false);
            return;
        }
        if (r.NextDouble() < Chunk.ore_lapis_chance && getPosition().y <= Chunk.ore_lapis_height)
        {
            Chunk.setBlock(getPosition(), Material.Structure_Block, "structure=Ore_Lapis|save=false", false);
            return;
        }
        if (r.NextDouble() < Chunk.ore_gold_chance && getPosition().y <= Chunk.ore_gold_height)
        {
            Chunk.setBlock(getPosition(), Material.Structure_Block, "structure=Ore_Gold|save=false", false);
            return;
        }
        if (r.NextDouble() < Chunk.ore_iron_chance && getPosition().y <= Chunk.ore_iron_height)
        {
            Chunk.setBlock(getPosition(), Material.Structure_Block, "structure=Ore_Iron|save=false", false);
            return;
        }
        if (r.NextDouble() < Chunk.ore_coal_chance && getPosition().y <= Chunk.ore_coal_height)
        {
            Chunk.setBlock(getPosition(), Material.Structure_Block, "structure=Ore_Coal|save=false", false);
            return;
        }
    }

    public override void Tick()
    {
        base.Tick();
    }
}
