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

    
    private const int OreCoalHeight = 128;
    private const double OreCoalChance = 0.004f;

    private const int OreIronHeight = 64;
    private const double OreIronChance = 0.002f;

    private const int OreGoldHeight = 32;
    private const double OreGoldChance = 0.001f;

    private const int OreLapisHeight = 32;
    private const double OreLapisChance = 0.001f;

    private const int OreRedstoneHeight = 16;
    private const double OreRedstoneChance = 0.002f;

    private const int OreDiamondHeight = 16;
    private const double OreDiamondChance = 0.0004f;
    
    public OverworldGenerator()
    {
        if(caveNoise == null)
            caveNoise = new Perlin(CaveFrequency, CaveLacunarity, CavePercistance, CaveOctaves,
                WorldManager.world.seed, QualityMode.High);
    }
    
    
    public override Material GenerateTerrainBlock(Location loc)
    {
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
        if (WorldManager.instance.caveHollowBlocks.Contains(loc))
            mat = Material.Air;

        //-Bedrock Generation-//
        if (loc.y <= 4)
        {
            System.Random r = new System.Random(SeedGenerator.SeedByLocation(loc));
            
            //Fill layer 0 and then progressively less chance of bedrock further up
            if (loc.y == 0)
                mat = Material.Bedrock;
            else if (r.Next(0, loc.y + 2) <= 1)
                mat = Material.Bedrock;
        }
        
        return mat;
    }

    public override BlockState GenerateStructures(Location loc, Biome biome)
    {
        System.Random r = new System.Random(SeedGenerator.SeedByLocation(loc));
        Material mat = loc.GetMaterial();
        Material matBeneath = (loc + new Location(0, -1)).GetMaterial();
        
        Material[] flowerMaterials = {Material.Red_Flower};
        Material[] vegetationMaterials = {Material.Tall_Grass};

        if ((matBeneath == Material.Grass || matBeneath == Material.Sand)  && mat == Material.Air) 
        {
            //Vegetation
            if (biome.name == "forest" || biome.name == "forest_hills" || biome.name == "birch_forest" ||
                biome.name == "plains")
                if (r.Next(0, 100) <= 50)
                    return new BlockState(vegetationMaterials[r.Next(0, vegetationMaterials.Length)]);

            if (biome.name == "forest" || biome.name == "forest_hills" || biome.name == "birch_forest" ||
                biome.name == "plains")
                if (r.Next(0, 100) <= 10)
                    return new BlockState(flowerMaterials[r.Next(0, flowerMaterials.Length)]);

            //Trees
            if (biome.name == "forest" || biome.name == "forest_hills")
                if (r.Next(0, 100) <= 20)
                    return new BlockState(Material.Structure_Block, new BlockData("structure=Oak_Tree"));

            //Birch Trees
            if (biome.name == "birch_forest")
                if (r.Next(0, 100) <= 20)
                    return new BlockState(Material.Structure_Block, new BlockData("structure=Birch_Tree"));

            //Unlikely Trees
            if (biome.name == "plains")
                if (r.Next(0, 100) <= 3)
                    return new BlockState(Material.Structure_Block, new BlockData("structure=Oak_Tree"));

            //Large Trees
            if (biome.name == "plains")
                if (r.Next(0, 100) <= 1)
                    return new BlockState(Material.Structure_Block, new BlockData("structure=Large_Oak_Tree"));

            //Dead Bushes
            if (biome.name == "desert")
                if (r.Next(0, 100) <= 8)
                    return new BlockState(Material.Dead_Bush);

            //Cactie
            if (biome.name == "desert")
                if (r.Next(0, 100) <= 5)
                    return new BlockState(Material.Structure_Block, new BlockData("structure=Cactus"));
        }

        //Generate Ores
        if (mat == Material.Stone)
        {
            if (r.NextDouble() < OreDiamondChance && loc.y <= OreDiamondHeight)
                return new BlockState(Material.Structure_Block, new BlockData("structure=Ore_Diamond"));
            if (r.NextDouble() < OreRedstoneChance && loc.y <= OreRedstoneHeight)
                return new BlockState(Material.Structure_Block, new BlockData("structure=Ore_Redstone"));
            if (r.NextDouble() < OreLapisChance && loc.y <= OreLapisHeight)
                return new BlockState(Material.Structure_Block, new BlockData("structure=Ore_Lapis"));
            if (r.NextDouble() < OreGoldChance && loc.y <= OreGoldHeight)
                return new BlockState(Material.Structure_Block, new BlockData("structure=Ore_Gold"));
            if (r.NextDouble() < OreIronChance && loc.y <= OreIronHeight)
                return new BlockState(Material.Structure_Block, new BlockData("structure=Ore_Iron"));
            if (r.NextDouble() < OreCoalChance && loc.y <= OreCoalHeight)
                return new BlockState(Material.Structure_Block, new BlockData("structure=Ore_Coal"));
        }
        
        //Generate Liquid Pockets
        if (loc.y < 40 && matBeneath == Material.Air && r.Next(0, 100) <= 3)
        {
            if (mat == Material.Stone)
            {
                Material liquidMat = (r.Next(0, 100) < 75 ? Material.Water : Material.Lava);

                return new BlockState(liquidMat);
            }
        }

        return new BlockState(Material.Air);
    }
}
