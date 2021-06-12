using System;
using System.Collections.Generic;
using LibNoise;
using LibNoise.Generator;
using UnityEngine;
using Random = System.Random;

[Serializable]
public class Biome
{
    [Space] [Header("Occourance Frequency")]
    public int biomeMinimumChunkSize;

    public int biomeMaximumChunkSize;


    [Space] public List<string> biomeSpecificEntitySpawns;

    public float landscapeHeightOverSeaLevel = 43;
    public float landscapeHeightWeight = 0.08f;

    [Space] [Header("Landscape")] public float landscapeLacunarity = 1f;

    public int landscapeOctaves = 2;
    public float landscapePercistance = 1f;
    public float landscapeSize = 0.05f;
    public string name = "";
    public float stoneLayerNoiseValue;

    public Perlin GetLandscapeNoise()
    {
        int seed = 0;
        if (WorldManager.world != null)
            seed = WorldManager.world.seed;

        if (WorldManager.instance != null)
            seed += WorldManager.instance.overworldBiomes.IndexOf(this);

        return new Perlin(1, landscapeLacunarity, landscapePercistance, landscapeOctaves, seed, QualityMode.Low);
    }

    public float GetLandscapeNoiseAt(Location loc)
    {
        float value = 100;
        if (loc.y > OverworldGenerator.SeaLevel - 20)
        {
            value = (float) GetLandscapeNoise().GetValue(loc.x * landscapeSize, loc.y * landscapeSize) + 4;
            value -= landscapeHeightWeight * (loc.y + landscapeHeightOverSeaLevel - OverworldGenerator.SeaLevel);
        }

        return value;
    }

    public static float BlendNoiseValues(Location loc, Biome biomeA, Biome biomeB, float weightA)
    {
        float weightB = 1 - weightA;

        weightA = Mathf.Clamp(weightA, 0, 1);
        weightB = Mathf.Clamp(weightB, 0, 1);

        float landscapeNoiseA = biomeA.GetLandscapeNoiseAt(loc) * weightA;
        float landscapeNoiseB = biomeB.GetLandscapeNoiseAt(loc) * weightB;

        return landscapeNoiseA + landscapeNoiseB;
    }

    public static Biome GetBiomeAt(ChunkPosition cPos)
    {
        if (WorldManager.instance.chunkBiomes.ContainsKey(cPos)
        ) //If biome value at this location already has been generated, use this value
            return WorldManager.instance.chunkBiomes[cPos];

        //Otherwise, generate the value, save it for later use, and return it
        GenerateBiomesForRegion(cPos);
        return WorldManager.instance.chunkBiomes[cPos];
    }

    public static void GenerateBiomesForRegion(ChunkPosition chunkPos)
    {
        Dimension dimension = chunkPos.dimension; //The dimension of the region
        int region; //region position

        if (chunkPos.chunkX >= 0)
            region = (int) (chunkPos.chunkX / (float) Chunk.AmountOfChunksInRegion);
        else
            region = Mathf.CeilToInt(((float) chunkPos.chunkX + 1) / Chunk.AmountOfChunksInRegion) - 1;

        int startChunk = region * Chunk.AmountOfChunksInRegion; //first chunk of the region
        int endChunk = startChunk + Chunk.AmountOfChunksInRegion; //last chunk of the region

        int currentChunk = startChunk; //start generating biome values at the first chunk of the region
        while (currentChunk < endChunk) //Keep on iterating until every chunk in the region has a biome
        {
            Random r = new Random(currentChunk.GetHashCode() + dimension.GetHashCode() +
                                  WorldManager.world
                                      .seed); //Random, with a seed unique to the chunk x position, the dimension, and the world seed
            Biome biome = null;
            if (dimension == Dimension.Overworld)
                biome = WorldManager.instance.overworldBiomes[
                    r.Next(0
                        , WorldManager.instance.overworldBiomes
                            .Count)]; //determine the biome that will be generated for the next few chunks at random
            else if (dimension == Dimension.Nether)
                biome = WorldManager.instance.netherBiome;

            int biomeChunkSize =
                r.Next(biome.biomeMinimumChunkSize
                    , biome.biomeMaximumChunkSize + 1); //how many chunks will this biome cover
            int biomeBeginningChunk = currentChunk; //where the biome starts

            while (currentChunk <= biomeBeginningChunk + biomeChunkSize
            ) //Assign this biome to every one of these chunks
            {
                if (currentChunk >= endChunk) //if chunk is outside of region, stop generating
                    break;

                ChunkPosition i = new ChunkPosition(currentChunk, dimension);
                if (WorldManager.instance.chunkBiomes.ContainsKey(i))
                    WorldManager.instance.chunkBiomes.Remove(i);

                WorldManager.instance.chunkBiomes.Add(i, biome);
                currentChunk++;
            }
        }
    }
}