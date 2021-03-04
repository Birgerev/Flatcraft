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

    public override void GenerateStructures(Location loc)
    {
        //TODO stop looking through conditions after finding a viable structure placement
        System.Random r = new System.Random(SeedGenerator.SeedByLocation(loc));
        Block block = loc.GetBlock();
        Material mat = Material.Air;
        Biome biome = Biome.GetBiomeAt(new ChunkPosition(loc)); //TODO biome could be calculated before loop, huge optimization
        if (block != null)
            mat = block.GetMaterial();
        
        Material[] flowerMaterials = {Material.Red_Flower};
        Material[] vegetationMaterials = {Material.Tall_Grass};

        if ((mat == Material.Grass || mat == Material.Sand) && (loc + new Location(0, 1)).GetBlock() == null)
        {
            //Vegetation
            if (biome.name == "forest" || biome.name == "forest_hills" || biome.name == "birch_forest" ||
                biome.name == "plains")
                if (r.Next(0, 100) <= 50)
                    (loc + new Location(0, 1)).SetMaterial(vegetationMaterials[r.Next(0, vegetationMaterials.Length)]);

            if (biome.name == "forest" || biome.name == "forest_hills" || biome.name == "birch_forest" ||
                biome.name == "plains")
                if (r.Next(0, 100) <= 10)
                    (loc + new Location(0, 1)).SetMaterial(flowerMaterials[r.Next(0, flowerMaterials.Length)]);

            //Trees
            if (biome.name == "forest" || biome.name == "forest_hills")
                if (r.Next(0, 100) <= 20)
                    (loc + new Location(0, 1)).SetMaterial(Material.Structure_Block)
                        .SetData(new BlockData("structure=Oak_Tree"));

            //Birch Trees
            if (biome.name == "birch_forest")
                if (r.Next(0, 100) <= 20)
                    (loc + new Location(0, 1)).SetMaterial(Material.Structure_Block)
                        .SetData(new BlockData("structure=Birch_Tree"));

            //Unlikely Trees
            if (biome.name == "plains")
                if (r.Next(0, 100) <= 3)
                    (loc + new Location(0, 1)).SetMaterial(Material.Structure_Block)
                        .SetData(new BlockData("structure=Oak_Tree"));

            //Large Trees
            if (biome.name == "plains")
                if (r.Next(0, 100) <= 1)
                    (loc + new Location(0, 1)).SetMaterial(Material.Structure_Block)
                        .SetData(new BlockData("structure=Large_Oak_Tree"));

            //Dead Bushes
            if (biome.name == "desert")
                if (r.Next(0, 100) <= 8)
                    (loc + new Location(0, 1)).SetMaterial(Material.Dead_Bush);

            //Cactie
            if (biome.name == "desert")
                if (r.Next(0, 100) <= 5)
                    (loc + new Location(0, 1)).SetMaterial(Material.Structure_Block)
                        .SetData(new BlockData("structure=Cactus"));
        }

        //Generate Ores
        if (mat == Material.Stone)
        {
            if (r.NextDouble() < OreDiamondChance && loc.y <= OreDiamondHeight)
                (loc + new Location(0, 1)).SetMaterial(Material.Structure_Block)
                    .SetData(new BlockData("structure=Ore_Diamond"));
            else if (r.NextDouble() < OreRedstoneChance && loc.y <= OreRedstoneHeight)
                (loc + new Location(0, 1)).SetMaterial(Material.Structure_Block)
                    .SetData(new BlockData("structure=Ore_Redstone"));
            else if (r.NextDouble() < OreLapisChance && loc.y <= OreLapisHeight)
                (loc + new Location(0, 1)).SetMaterial(Material.Structure_Block)
                    .SetData(new BlockData("structure=Ore_Lapis"));
            else if (r.NextDouble() < OreGoldChance && loc.y <= OreGoldHeight)
                (loc + new Location(0, 1)).SetMaterial(Material.Structure_Block)
                    .SetData(new BlockData("structure=Ore_Gold"));
            else if (r.NextDouble() < OreIronChance && loc.y <= OreIronHeight)
                (loc + new Location(0, 1)).SetMaterial(Material.Structure_Block)
                    .SetData(new BlockData("structure=Ore_Iron"));
            else if (r.NextDouble() < OreCoalChance && loc.y <= OreCoalHeight)
                (loc + new Location(0, 1)).SetMaterial(Material.Structure_Block)
                    .SetData(new BlockData("structure=Ore_Coal"));
        }
        
        //Generate Liquid Pockets
        if (loc.y < 50 && mat == Material.Air && r.Next(0, 100) <= 5)
        {
            if ((loc + new Location(0, 1)).GetMaterial() == Material.Stone)
            {
                Material liquidMat = (r.Next(0, 100) < 75 ? Material.Water : Material.Lava);
                
                (loc + new Location(0, 1)).SetMaterial(liquidMat);
            }
        }
    }
}
