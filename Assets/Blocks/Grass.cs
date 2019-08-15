using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : Block
{
    public static string default_texture = "block_grass";
    public override float breakTime { get; } = 0.75f;

    public override void FirstTick()
    {
        base.FirstTick();

        if (Chunk.getBlock(getPosition() + new Vector2Int(0, 1)) == null)
        {
            System.Random r = new System.Random(Chunk.seedByPosition(getPosition()));

            //Generate Structures
            if (r.Next(0, 100) <= 5)
            {
                Chunk.setBlock(getPosition() + new Vector2Int(0, 1), Material.Structure_Block, "structure=Tree,save=false", false);
            }

            //Vegetation
            if (r.Next(0, 100) <= 25)
            {
                Material[] vegetationMaterials = new Material[] { Material.Tall_Grass, Material.Red_Flower };

                Chunk.setBlock(getPosition() + new Vector2Int(0, 1), vegetationMaterials[r.Next(0, vegetationMaterials.Length)], false);
            }
        }
    }

    public override void Tick()
    {
        base.Tick();

        //If not covered by a block
        if(Chunk.getBlock(getPosition() + new Vector2Int(0, 1)) != null)
        {
            //Turn to dirt if covered
            if(Chunk.getBlock(getPosition() + new Vector2Int(0, 1)).playerCollide)
                Chunk.setBlock(getPosition(), Material.Dirt, false);
        }
    }
}
