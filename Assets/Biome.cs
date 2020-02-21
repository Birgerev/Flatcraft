using System.Collections;
using System.Collections.Generic;
using LibNoise;
using UnityEngine;

[System.Serializable]
public class Biome
{
    public static int averageBlocks = 1;

    [Space]
    [Header("Landscape")]
    public float landscapeLacunarity = 1f;
    public float landscapePercistance = 1f;
    public int landscapeOctaves = 2;
    public float landscapeSize = 0.05f;
    public float landscapeHeightWeight = 0.08f;
    public float landscapeHeightOverSeaLevel = 43;

    [Space]
    [Header("Biome")]
    public float biomeLacunarity = 1f;
    public float biomePercistance = 1f;
    public int biomeOctaves = 2;
    public float biomeDiminishBelow = 20;
    public float biomeDiminishLowWeight = 0.05f;
    public float biomeDiminishAbove = 80;
    public float biomeDiminishHighWeight = 0.05f;
    public float biomeAmplitude = 10f;


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
        float value = (float)getLandscapeNoise().GetValue(loc.x * landscapeSize, loc.y * landscapeSize) + 4;
        if (loc.y > Chunk.sea_level - 10)
        {
            value -= (landscapeHeightWeight * ((float)loc.y + landscapeHeightOverSeaLevel - (Chunk.sea_level)));
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

        return new LibNoise.Generator.Perlin(1, biomeLacunarity, biomePercistance, biomeOctaves, seed, QualityMode.Low);
    }

    public float getBiomeValueAt(int x)
    {
        float value = 0;
        value = ((float)getBiomeNoise().GetValue(x, 0) + 4);

        if (value < biomeDiminishBelow)
            value -= -(biomeDiminishBelow - value) * biomeDiminishLowWeight;

        if (value > biomeDiminishAbove)
            value -= (value - biomeDiminishAbove) * biomeDiminishHighWeight;

        value *= biomeAmplitude;

        return value;
    }

    public float getAverageBiomeValueAt(int x)
    {
        float value = 0;

        for(int i = -Mathf.FloorToInt((float)(averageBlocks) / 2f); i <= averageBlocks - (Mathf.CeilToInt((float)(averageBlocks) / 2f)); i++)
        {
            value += getBiomeValueAt(i + x);
        }

        value /= averageBlocks;

        return value;
    }

    public float blendNoiseValues(Biome secondBiome, Location loc)
    {
        float biomeNoiseA = getAverageBiomeValueAt(loc.x);
        float biomeNoiseB = secondBiome.getAverageBiomeValueAt(loc.x);

        float weightA = biomeNoiseA / (biomeNoiseA + biomeNoiseB);
        float weightB = 1 - weightA;

        weightA = Mathf.Clamp(weightA, 0, 1);
        weightB = Mathf.Clamp(weightB, 0, 1);

        float landscapeNoiseA = getLandscapeNoiseAt(loc) * weightA;
        float landscapeNoiseB = secondBiome.getLandscapeNoiseAt(loc) * weightB;

        return landscapeNoiseA + landscapeNoiseB;
    }
}
