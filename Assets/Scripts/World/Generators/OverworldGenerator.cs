using LibNoise;
using LibNoise.Generator;
using UnityEngine;
using Random = System.Random;

public class OverworldGenerator : WorldGenerator
{
    private const int MaxStrongholdDistance = 4000;
    private const float CaveFrequency = 5;
    private const float CaveLacunarity = 0.6f;
    private const float CavePercistance = 2;
    private const int CaveOctaves = 4;
    private const int LavaHeight = 10;


    private const int OreCoalHeight = 128;
    private const double OreCoalChance = 0.0035d;

    private const int OreIronHeight = 64;
    private const double OreIronChance = 0.0025d;

    private const int OreGoldHeight = 32;
    private const double OreGoldChance = 0.0008d;

    private const int OreLapisHeight = 32;
    private const double OreLapisChance = 0.001d;

    private const int OreRedstoneHeight = 16;
    private const double OreRedstoneChance = 0.002d;

    private const double DesertTempleChance = 0.005d;
    private const double DungeonChance = 0.0002d;
    private const double MineshaftChance = 0.0002d;
    private const int OreDiamondHeight = 16;
    private const double OreDiamondChance = 0.0005d;
    public static int SeaLevel = 62;
    public static Perlin materialPatchNoise;

    public OverworldGenerator()
    {
        if (materialPatchNoise == null)
            materialPatchNoise = new Perlin(CaveFrequency, CaveLacunarity, CavePercistance, CaveOctaves,
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
                1 - Mathf.Abs(loc.x - new ChunkPosition(loc).worldX) / (float) Chunk.Width;
            noiseValue = Biome.BlendNoiseValues(loc, biome, rightBiome, primaryBiomeWeight);
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
                mat = Material.Grass_Block;
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
            if (Mathf.Abs((float) materialPatchNoise.GetValue((float) loc.x / 20, (float) loc.y / 20)) > 7.5f)
                mat = Material.Dirt;
            if (Mathf.Abs((float) materialPatchNoise.GetValue((float) loc.x / 20 + 100, (float) loc.y / 20, 200)) > 7.5f)
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
            Random r = new Random(SeedGenerator.SeedByWorldLocation(loc));

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
        Random r = new Random(SeedGenerator.SeedByWorldLocation(loc));
        Material mat = loc.GetMaterial();
        Material matBeneath = (loc + new Location(0, -1)).GetMaterial();

        Material[] flowerMaterials = {Material.Red_Flower, Material.Yellow_Flower};
        
        //Debug Structure
        if(loc.x == 0 && loc.y == 50)
            return new BlockState(Material.Structure_Block, new BlockData("structure=Debug"));
        
        //Topmost Terrain Blocks
        if ((matBeneath == Material.Grass_Block || matBeneath == Material.Sand) && mat == Material.Air)
        {
            //Sugar canes
            if (loc.y == SeaLevel + 1)
            {
                Location belowLeft = loc + new Location(-1, -1);
                Location belowRight = loc + new Location(1, -1);
                
                if(belowLeft.GetMaterial() == Material.Water || belowRight.GetMaterial() == Material.Water)
                    if (r.Next(0, 100) <= 25)
                        return new BlockState(Material.Structure_Block, new BlockData("structure=Sugar_Cane"));
            }
            
            //Grass
            if (biome.name == "forest" || biome.name == "forest_hills" || biome.name == "birch_forest" ||
                biome.name == "plains")
                if (r.Next(0, 100) <= 50)
                    return new BlockState(Material.Grass);
            
            //Tall grass
            if (biome.name == "forest" || biome.name == "forest_hills" || biome.name == "birch_forest" ||
                biome.name == "plains")
                if (r.Next(0, 100) <= 20)
                    return new BlockState(Material.Tall_Grass);

            //Flowers
            if (biome.name == "forest" || biome.name == "forest_hills" || biome.name == "birch_forest" ||
                biome.name == "plains")
                if (r.Next(0, 100) <= 10)
                    return new BlockState(flowerMaterials[r.Next(0, flowerMaterials.Length)]);

            //Forest Trees
            if (biome.name == "forest" || biome.name == "forest_hills")
                if (r.Next(0, 100) <= 25)
                    return new BlockState(Material.Structure_Block, new BlockData("structure=Oak_Tree"));

            //Birch Forest Trees
            if (biome.name == "birch_forest")
                if (r.Next(0, 100) <= 25)
                    return new BlockState(Material.Structure_Block, new BlockData("structure=Birch_Tree"));

            //Plains Trees
            if (biome.name == "plains")
                if (r.Next(0, 100) <= 3)
                    return new BlockState(Material.Structure_Block, new BlockData("structure=Oak_Tree"));

            //Large Oak Trees
            if (biome.name == "forest" || biome.name == "forest_hills" || biome.name == "birch_forest" || 
                biome.name == "plains")
                if (r.Next(0, 100) <= 2)
                    return new BlockState(Material.Structure_Block, new BlockData("structure=Large_Oak_Tree"));

            //Dead Bushes
            if (biome.name == "desert")
                if (r.Next(0, 100) <= 8)
                    return new BlockState(Material.Dead_Bush);

            //Cactie
            if (biome.name == "desert")
                if (r.Next(0, 100) <= 5)
                    return new BlockState(Material.Structure_Block, new BlockData("structure=Cactus"));
            //TODO streamline cactus & sugarcane generation
            
            //Desert Temple
            if (biome.name == "desert")
                if (r.NextDouble() < DesertTempleChance)
                    return new BlockState(Material.Structure_Block, new BlockData("structure=Desert_Temple"));
            
        }

        if(loc.y == 10 && loc.x == GetStrongholdLocation())
            return new BlockState(Material.Structure_Block, new BlockData("structure=Stronghold/Presets"));
        
        //Generate Liquid Pockets
        if (loc.y < 40 && matBeneath == Material.Air && r.Next(0, 100) <= 2)
            if (mat == Material.Stone)
            {
                Material liquidMat = r.Next(0, 100) < 75 ? Material.Water : Material.Lava;

                return new BlockState(liquidMat);
            }
        
        //Lava lakes
        if (loc.y < LavaHeight && mat == Material.Air)
            return new BlockState(Material.Lava);
        
        if (mat == Material.Stone)
        {
            //Generate Ores
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
            
            
            //Generate Dungeon
            if (r.NextDouble() < DungeonChance)
                return new BlockState(Material.Structure_Block, new BlockData("structure=Dungeon"));//Generate floors separately
            
            //Generate Mineshaft
            if (r.NextDouble() < MineshaftChance && loc.y > 30 && loc.y < 50)
                return new BlockState(Material.Structure_Block, new BlockData("structure=Mineshaft/Presets"));
        }

        return new BlockState(Material.Air);
    }

    public static int GetStrongholdLocation()
    {
        Random r = new Random(WorldManager.world.seed);
        
        return r.Next(-MaxStrongholdDistance, MaxStrongholdDistance);
    }
}