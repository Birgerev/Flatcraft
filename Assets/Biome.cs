using System.Collections;
using System.Collections.Generic;
using LibNoise;
using UnityEngine;

[System.Serializable]
public class Biome
{
    public float previewSize = 100;
    private float size = 0.05f;

    [Space]
    [Header("Landscape")]
    public AnimationCurve previewLandscapeCurve;

    public float landscapeFrequency = 1f;
    public float landscapeLacunarity = 1f;
    public float landscapePercistance = 1f;
    public int landscapeOctaves = 2;

    [Space]
    [Header("Biome")]
    public AnimationCurve biomeCurve;


    [Space]
    public string name = "";
    public float heightWeight = 0.08f;
    public float heightOverSeaLevel = 43;

    public LibNoise.Generator.Perlin getLandscapeNoise()
    {
        int seed = 0;
        if (WorldManager.world != null)
            seed = WorldManager.world.seed;

        seed += WorldManager.instance.biomes.IndexOf(this);

        return new LibNoise.Generator.Perlin(landscapeFrequency, landscapeLacunarity, landscapePercistance, landscapeOctaves, seed, QualityMode.Low);
    }

    public float getLandscapeNoiseAt(Vector2Int pos)
    {
        float value = (float)getLandscapeNoise().GetValue(pos.x * size, 0) + 4;
        if (pos.y > Chunk.sea_level - 10)
        {
            value -= (heightWeight * ((float)pos.y + heightOverSeaLevel - (Chunk.sea_level)));
        }
        return value;
    }
    
    public float getBiomeValueAt(int x)
    {
        return biomeCurve.Evaluate(x % biomeCurve.keys[biomeCurve.keys.Length-1].time);
    }

    public void GenerateCurves()
    {
        previewLandscapeCurve = new AnimationCurve();

        for (int i = 0; i < previewSize; i++)
        {
            previewLandscapeCurve.AddKey(i, getLandscapeNoiseAt(new Vector2Int(i, 0)));
        }
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
