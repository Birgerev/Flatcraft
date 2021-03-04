using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetherGenerator : WorldGenerator
{
    public const int LavaLevel = 62;
    
    public override Material GenerateTerrainBlock(Location loc)
    {
        System.Random r = new System.Random(SeedGenerator.SeedByLocation(loc));
        ChunkPosition cPos = new ChunkPosition(loc);
        Biome biome = Biome.GetBiomeAt(cPos);
        
        Material mat = Material.Air;
        float noiseValue = biome.GetLandscapeNoiseAt(loc);
        
        if (noiseValue > 0.1f)
            mat = Material.Netherrack;

        if (mat == Material.Air && loc.y <= LavaLevel) 
            mat = Material.Lava;


        //-Lower Bedrock Generation-//
        if (loc.y <= 4)
        {
            //Fill layer 0 and then progressively less chance of bedrock further up
            if (loc.y == 0)
                mat = Material.Bedrock;
            else if (r.Next(0, loc.y + 2) <= 1)
                mat = Material.Bedrock;
        }
        
        //-Upper Bedrock Generation-//
        if (loc.y >= 128 - 4 && loc.y <= 128)
        {
            //Fill layer 256 and then progressively less chance of bedrock further down
            if (loc.y == 128 - 4)
                mat = Material.Bedrock;
            else if (r.Next(0, (128 - loc.y) + 2) <= 1)
                mat = Material.Bedrock;
        }

        if (loc.y > 128)
            mat = Material.Air;

        return mat;
    }
}
