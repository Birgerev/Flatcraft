using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibNoise;
using LibNoise.Generator;

public class OverworldGenerator : WorldGenerator
{
    public static int SeaLevel = 62;
    public static Perlin caveNoise = null;

    private const float CaveFrequency = 5;
    private const float CaveLacunarity = 0.6f;
    private const float CavePercistance = 2;
    private const int CaveOctaves = 4;
    private const float CaveHollowValue = 2.2f;
    private const int LavaHeight = 10;

    public OverworldGenerator()
    {
        if(caveNoise == null)
            caveNoise = new Perlin(CaveFrequency, CaveLacunarity, CavePercistance, CaveOctaves,
                WorldManager.world.seed, QualityMode.High);
    }
    
    
    public override Material GenerateTerrainBlock(Location loc)
    {
        System.Random r = new System.Random(SeedGenerator.SeedByLocation(loc));
        ChunkPosition cPos = new ChunkPosition(loc);
        
        Material mat = Material.Air;
        float noiseValue;
        
        //Determine wheights for each biome
        Biome biome = Biome.GetBiomeAt(cPos);
        Biome rightBiome = Biome.GetBiomeAt(new ChunkPosition(cPos.chunkX + 1, cPos.dimension));
        Biome leftBiome = Biome.GetBiomeAt(new ChunkPosition(cPos.chunkX - 1, cPos.dimension));
        
        float primaryBiomeWeight;
        if (biome != rightBiome)
        {
            primaryBiomeWeight = 
                0.5f - (float)Mathf.Abs(loc.x - new ChunkPosition(loc).chunkX * Chunk.Width) / Chunk.Width / 2f;
            noiseValue = Biome.BlendNoiseValues(loc, biome, rightBiome, primaryBiomeWeight);
        }
        else if (biome != leftBiome)
        {
            primaryBiomeWeight = 
                0.5f + (float)Mathf.Abs(loc.x - new ChunkPosition(loc).chunkX * Chunk.Width) / Chunk.Width / 2f;
            noiseValue = Biome.BlendNoiseValues(loc, biome, leftBiome, primaryBiomeWeight);
        }
        else
        {
            noiseValue = biome.GetLandscapeNoiseAt(loc);
        }
        
        
        //-Terrain-//
        if (noiseValue > 0.1f)
        {
            if (biome.name == "desert")
            {
                mat = Material.Sand;
                if (noiseValue > biome.stoneLayerNoiseValue - 2)
                    mat = Material.Sandstone;
            }
            else if (biome.name == "forest" || biome.name == "forest_hills" || biome.name == "birch_forest" ||
                     biome.name == "plains")
            {
                mat = Material.Grass;
            }

            if (noiseValue > biome.stoneLayerNoiseValue) 
                mat = Material.Stone;
        }

        //-Lakes-//
        if (mat == Material.Air && loc.y <= SeaLevel) 
            mat = Material.Water;

        //-Dirt & Gravel Patches-//
        if (mat == Material.Stone)
        {
            if (Mathf.Abs((float)caveNoise.GetValue((float)loc.x / 20, (float)loc.y / 20)) > 7.5f)
                mat = Material.Dirt;
            if (Mathf.Abs((float)caveNoise.GetValue((float)loc.x / 20 + 100, (float)loc.y / 20, 200)) > 7.5f)
                mat = Material.Gravel;
        }

        //-Sea-//
        if (mat == Material.Air && loc.y <= SeaLevel) 
            mat = Material.Water;

        //-Caves-//
        if (noiseValue > 0.1f)
        {
            var caveValue = (caveNoise.GetValue((float)loc.x / 20, (float)loc.y / 20) + 4.0f) / 4f;
            if (caveValue > CaveHollowValue)
            {
                mat = Material.Air;

                //-Lava Lakes-//
                if (loc.y <= LavaHeight)
                    mat = Material.Lava;
            }
        }

        //-Bedrock Generation-//
        if (loc.y <= 4)
        {
            //Fill layer 0 and then progressively less chance of bedrock further up
            if (loc.y == 0)
                mat = Material.Bedrock;
            else if (r.Next(0, loc.y + 2) <= 1)
                mat = Material.Bedrock;
        }
        
        return mat;
    }
}
