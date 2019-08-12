using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone : Block
{
    public override string texture { get; } = "block_stone";
    public override float breakTime { get; } = 6;

    public override void FirstTick()
    {
        base.FirstTick();
        
        System.Random r = new System.Random(Chunk.seedByPosition(getPosition()));

        //Generate Ores
        if (r.NextDouble() < Chunk.ore_coal_chance && getPosition().y <= Chunk.ore_coal_height)
        {
            Chunk.setBlock(getPosition(), Material.Structure_Block, "structure=Ore_Coal,save=false", false);
        }
    }

    public override void Tick()
    {
        base.Tick();
    }
}
