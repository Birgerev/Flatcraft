using System.Collections;
using System.Collections.Generic;
using LibNoise;
using UnityEngine;

[System.Serializable]
public class Biome
{

    [Space]
    [Header("Landscape")]
    public float landscapeFrequency = 1f;
    public float landscapeLacunarity = 1f;
    public float landscapePercistance = 1f;
    public int landscapeOctaves = 2;
    public float landscapeSize = 0.05f;

    [Space]
    [Header("Biome")]
    public float biomeFrequency = 1f;
    public float biomeLacunarity = 1f;
    public float biomePercistance = 1f;
    public int biomeOctaves = 2;
    public float biomeAmplitude = 10f;


    [Space]
    public string name = "";
    public float heightWeight = 0.08f;
    public float heightOverSeaLevel = 43;

    public LibNoise.Generator.Perlin getLandscapeNoise()
    {
        int seed = 0;
        if (WorldManager.world != null)
            seed = WorldManager.world.seed;

        if(WorldManager.instance != null)
            seed += WorldManager.instance.biomes.IndexOf(this);

        return new LibNoise.Generator.Perlin(landscapeFrequency, landscapeLacunarity, landscapePercistance, landscapeOctaves, seed, QualityMode.Low);
    }

    public float getLandscapeNoiseAt(Vector2Int pos)
    {
        float value = (float)getLandscapeNoise().GetValue(pos.x * landscapeSize, pos.y * landscapeSize) + 4;
        if (pos.y > Chunk.sea_level - 10)
        {
            value -= (heightWeight * ((float)pos.y + heightOverSeaLevel - (Chunk.sea_level)));
        }
        return value;
    }

    public LibNoise.Generator.Perlin getBiomeNoise()
    {
        int seed = 0;
        if (WorldManager.world != null)
            seed = WorldManager.world.seed;

        if (WorldManager.instance != null)
            seed += WorldManager.instance.biomes.IndexOf(this);

        seed += 1000;

        return new LibNoise.Generator.Perlin(biomeFrequency, biomeLacunarity, biomePercistance, biomeOctaves, seed, QualityMode.Low);
    }

    public float getBiomeValueAt(int x)
    {
        return ((float)getBiomeNoise().GetValue(x, 0) + 4) * biomeAmplitude;
    }

    public float blendNoiseValues(Biome secondBiome, Vector2Int pos)
    {
        float biomeNoiseA = getBiomeValueAt(pos.x);
        float biomeNoiseB = secondBiome.getBiomeValueAt(pos.x);

        float weightA = biomeNoiseA / (biomeNoiseA + biomeNoiseB);
        float weightB = 1 - weightA;
        

        float landscapeNoiseA = getLandscapeNoiseAt(pos) * weightA;
        float landscapeNoiseB = secondBiome.getLandscapeNoiseAt(pos) * weightB;

        return landscapeNoiseA + landscapeNoiseB;
    }
}
