using System;
using System.Collections.Generic;
using LibNoise;
using LibNoise.Generator;
using UnityEngine;
using Random = System.Random;

[Serializable]
public class Biome
{
    public int biomeMaximumChunkSize;

    [Space] [Header("Occourance Frequency")]
    public int biomeMinimumChunkSize;


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
        var seed = 0;
        if (WorldManager.world != null)
            seed = WorldManager.world.seed;

        if (WorldManager.instance != null)
            seed += WorldManager.instance.biomes.IndexOf(this);

        return new Perlin(1, landscapeLacunarity, landscapePercistance, landscapeOctaves, seed, QualityMode.Low);
    }

    public float GetLandscapeNoiseAt(Location loc)
    {
        float value = 100;
        if (loc.y > Chunk.SeaLevel - 20)
        {
            value = (float) GetLandscapeNoise().GetValue(loc.x * landscapeSize, loc.y * landscapeSize) + 4;
            value -= landscapeHeightWeight * (loc.y + landscapeHeightOverSeaLevel - Chunk.SeaLevel);
        }

        return value;
    }

    public static float BlendNoiseValues(Location loc, Biome biomeA, Biome biomeB, float weightA)
    {
        var weightB = 1 - weightA;

        weightA = Mathf.Clamp(weightA, 0, 1);
        weightB = Mathf.Clamp(weightB, 0, 1);

        var landscapeNoiseA = biomeA.GetLandscapeNoiseAt(loc) * weightA;
        var landscapeNoiseB = biomeB.GetLandscapeNoiseAt(loc) * weightB;

        return landscapeNoiseA + landscapeNoiseB;
    }

    public static Biome GetBiomeAt(ChunkPosition cPos)
    {
        if (WorldManager.instance.chunkBiomes.ContainsKey(cPos)) //If biome value at this location already has been generated, use this value
            return WorldManager.instance.chunkBiomes[cPos];

        //Otherwise, generate the value, save it for later use, and return it
        GenerateBiomesForRegion(cPos);
        return WorldManager.instance.chunkBiomes[cPos];
    }

    public static void GenerateBiomesForRegion(ChunkPosition chunkPos)
    {
        var dimension = chunkPos.dimension; //The dimension of the region
        int region; //region position

        if (chunkPos.chunkX >= 0)
            region = (int) (chunkPos.chunkX / (float) Chunk.AmountOfChunksInRegion);
        else
            region = Mathf.CeilToInt(((float) chunkPos.chunkX + 1) / Chunk.AmountOfChunksInRegion) - 1;

        var startChunk = region * Chunk.AmountOfChunksInRegion; //first chunk of the region
        var endChunk = startChunk + Chunk.AmountOfChunksInRegion; //last chunk of the region

        var currentChunk = startChunk; //start generating biome values at the first chunk of the region
        while (currentChunk < endChunk) //Keep on iterating until every chunk in the region has a biome
        {
            var r = new Random(currentChunk.GetHashCode() + dimension.GetHashCode() +
                               WorldManager.world
                                   .seed); //Random, with a seed unique to the chunk x position, the dimension, and the world seed
            var biome = WorldManager.instance.biomes[
                r.Next(0,
                    WorldManager.instance.biomes
                        .Count)]; //determine the biome that will be generated for the next few chunks at random
            var biomeChunkSize =
                r.Next(biome.biomeMinimumChunkSize,
                    biome.biomeMaximumChunkSize); //how many chunks will this biome cover
            var biomeBeginningChunk = currentChunk; //where the biome starts

            while (currentChunk <= biomeBeginningChunk + biomeChunkSize
            ) //Assign this biome to every one of these chunks
            {
                if (currentChunk >= endChunk) //if chunk is outside of region, stop generating
                    break;

                var i = new ChunkPosition(currentChunk, dimension);
                if (WorldManager.instance.chunkBiomes.ContainsKey(i))
                    WorldManager.instance.chunkBiomes.Remove(i);

                WorldManager.instance.chunkBiomes.Add(i, biome);
                currentChunk++;
            }
        }
    }
}