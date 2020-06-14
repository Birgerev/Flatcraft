using System.Collections;
using System.Collections.Generic;
using LibNoise;
using UnityEngine;

[System.Serializable]
public class Biome
{
    [Space]
    [Header("Landscape")]
    public float landscapeLacunarity = 1f;
    public float landscapePercistance = 1f;
    public int landscapeOctaves = 2;
    public float landscapeSize = 0.05f;
    public float landscapeHeightWeight = 0.08f;
    public float landscapeHeightOverSeaLevel = 43;
    public float stoneLayerNoiseValue;

    [Space] [Header("Occourance Frequency")]
    public int biomeMinimumChunkSize;
    public int biomeMaximumChunkSize;


    [Space]
    public string name = "";
    public List<string> biomeSpecificEntitySpawns;

    public LibNoise.Generator.Perlin getLandscapeNoise()
    {
        int seed = 0;
        if (WorldManager.world != null)
            seed = WorldManager.world.seed;

        if(WorldManager.instance != null)
            seed += WorldManager.instance.biomes.IndexOf(this);

        return new LibNoise.Generator.Perlin(1, landscapeLacunarity, landscapePercistance, landscapeOctaves, seed, QualityMode.Low);
    }

    public float getLandscapeNoiseAt(Location loc)
    {
        float value = 100;
        if (loc.y > Chunk.SeaLevel - 20)
        {
            value = (float) getLandscapeNoise().GetValue(loc.x * landscapeSize, loc.y * landscapeSize) + 4;
            value -= (landscapeHeightWeight * ((float)loc.y + landscapeHeightOverSeaLevel - (Chunk.SeaLevel)));
        }
        return value;
    }

    public static float blendNoiseValues(Location loc, Biome biomeA, Biome biomeB, float weightA)
    {
        float weightB = 1 - weightA;

        weightA = Mathf.Clamp(weightA, 0, 1);
        weightB = Mathf.Clamp(weightB, 0, 1);

        float landscapeNoiseA = biomeA.getLandscapeNoiseAt(loc) * weightA;
        float landscapeNoiseB = biomeB.getLandscapeNoiseAt(loc) * weightB;

        return landscapeNoiseA + landscapeNoiseB;
    }

    public static Biome getBiomeAt(ChunkPosition cPos)
    {
        if (WorldManager.instance.chunkBiomes.ContainsKey(cPos))    //If biome value at this location already has been generated, use this value
        {
            return WorldManager.instance.chunkBiomes[cPos];
        }
        
        //Otherwise, generate the value, save it for later use, and return it
        generateBiomesForRegion(cPos);
        return WorldManager.instance.chunkBiomes[cPos];
    }
    
    public static void generateBiomesForRegion(ChunkPosition chunkPos)
    {
        Dimension dimension = chunkPos.dimension;    //The dimension of the region
        int region;    //region position

        if (chunkPos.chunkX >= 0)
            region = chunkPos.chunkX / Chunk.AmountOfChunksInRegion;
        else
            region = (chunkPos.chunkX / Chunk.AmountOfChunksInRegion) - 1;
                
        int startChunk = region * Chunk.AmountOfChunksInRegion;    //first chunk of the region
        int endChunk = startChunk + Chunk.AmountOfChunksInRegion;    //last chunk of the region

        int currentChunk = startChunk;    //start generating biome values at the first chunk of the region
        while (currentChunk < endChunk)    //Keep on iterating until every chunk in the region has a biome
        {
            System.Random r = new System.Random(currentChunk.GetHashCode() + dimension.GetHashCode() + WorldManager.world.seed);//Random, with a seed unique to the chunk x position, the dimension, and the world seed
            Biome biome = WorldManager.instance.biomes[r.Next(0, WorldManager.instance.biomes.Count)];    //determine the biome that will be generated for the next few chunks at random
            int biomeChunkSize = r.Next(biome.biomeMinimumChunkSize, biome.biomeMaximumChunkSize);    //how many chunks will this biome cover
            int biomeBeginningChunk = currentChunk;    //where the biome starts

            while(currentChunk <= biomeBeginningChunk + biomeChunkSize)    //Assign this biome to every one of these chunks
            {
                if (currentChunk >= endChunk)    //if chunk is outside of region, stop generating
                    break;
                
                ChunkPosition i = new ChunkPosition(currentChunk, dimension);
                WorldManager.instance.chunkBiomes[i] = biome;
                currentChunk++;//just updated this loop, try playing the game
            }
        }
    }
}
