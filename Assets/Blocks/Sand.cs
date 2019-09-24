using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sand : Block
{
    public static string default_texture = "block_sand";
    public override float breakTime { get; } = 0.75f;

    public override Tool_Type propperToolType { get; } = Tool_Type.Shovel;
    
    public override void GeneratingTick()
    {
        base.GeneratingTick();

        if (Chunk.getBlock(getPosition() + new Vector2Int(0, 1)) == null)
        {
            System.Random r = new System.Random(Chunk.seedByPosition(getPosition()));

            //Generate Structures
            if (r.Next(0, 100) <= 8)
            {
                Chunk.setBlock(getPosition() + new Vector2Int(0, 1), Material.Structure_Block, "structure=Cactus|save=false", false);
            }
        }
    }

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
