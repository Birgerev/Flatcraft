using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using LibNoise;
using LibNoise.Generator;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using Random = System.Random;

[BurstCompile]
public class Chunk : MonoBehaviour
{
    public const int Width = 16, Height = 255;
    public const int RenderDistance = 4;
    public const int AmountOfChunksInRegion = 16;
    public const int SpawnChunkDistance = 0;
    public const int OutsideRenderDistanceUnloadTime = 10;
    public const int TickRate = 1;

    [Header("Cave Generation Settings")] private const float CaveFrequency = 5;

    private const float CaveLacunarity = 0.6f;
    private const float CavePercistance = 2;
    private const int CaveOctaves = 4;
    private const float CaveHollowValue = 2.2f;

    [Header("Ore Generation Settings")] private const int OreCoalHeight = 128;

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

    private const int LavaHeight = 10;
    public const int SeaLevel = 62;

    private static readonly float mobSpawningChance = 0.01f;
    private static readonly List<string> MobSpawnTypes = new List<string> {"Chicken", "Sheep"};
    public int age;

    public GameObject blockPrefab;

    public Dictionary<int2, Block> blocks = new Dictionary<int2, Block>();

    private Perlin caveNoise;

    public ChunkPosition chunkPosition;
    public bool isLoaded;
    public bool isLoading;
    public bool isSpawnChunk;

    public HashSet<Location> lightSourceToUpdate = new HashSet<Location>();
    private Perlin patchNoise;

    private void Start()
    {
        isSpawnChunk = chunkPosition.chunkX >= -SpawnChunkDistance && chunkPosition.chunkX <= SpawnChunkDistance;

        WorldManager.instance.chunks.Add(chunkPosition, this);

        StartCoroutine(SelfDestructionChecker());

        gameObject.name = "Chunk [" + chunkPosition.chunkX + "]";
        transform.position = new Vector3(chunkPosition.worldX, 0, 0);


        StartCoroutine(GenerateChunk());
    }

    public void DestroyChunk()
    {
        if (isSpawnChunk)
            return;

        WorldManager.instance.chunks.Remove(chunkPosition);

        if (isLoading)
            WorldManager.instance.amountOfChunksLoading--;

        Destroy(gameObject);
    }

    public static void CreateChunksAround(Location loc, int distance)
    {
        for (var i = -distance; i < distance; i++)
        {
            var cPos = new ChunkPosition((int) (loc.x / (float) Width) + i, loc.dimension);
            if (!cPos.IsChunkLoaded())
                cPos.CreateChunk();
        }
    }

    private void GeneratingTickAllBlocks()
    {
        //Tick Blocks
        var blockList = transform.GetComponentsInChildren<Block>();

        foreach (var block in blockList)
        {
            if (block == null || block.transform == null)
                continue;

            block.GeneratingTick();
        }
    }

    private IEnumerator AutosaveAllBlocks()
    {
        var blocks = new List<Block>(this.blocks.Values);

        if (blocks.Count > 0)
        {
            var blocksPerBatch = 20;
            var timePerBatch = 5f / (blocks.Count / (float) blocksPerBatch);
            foreach (var block in blocks)
            {
                yield return new WaitForSeconds(timePerBatch);
                if (block == null || !block.autosave)
                    continue;


                block.Tick();
                block.Autosave();
            }
        }
    }

    private IEnumerator SelfDestructionChecker()
    {
        while (true)
        {
            var timePassedOutsideRenderDistance = 0f;
            while (!chunkPosition.isInRenderDistance())
            {
                yield return new WaitForSeconds(1f);
                timePassedOutsideRenderDistance += 1f;
                if (timePassedOutsideRenderDistance > OutsideRenderDistanceUnloadTime)
                {
                    DestroyChunk();
                    yield break;
                }
            }

            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator Tick()
    {
        while (true)
        {
            //Update neighbor chunks
            if (age < 5) TrySpawnMobs();

            age++;
            yield return new WaitForSeconds(1f / TickRate);
        }
    }

    public Chunk GetRightChunk()
    {
        return new ChunkPosition(chunkPosition.chunkX + 1, chunkPosition.dimension).GetChunk();
    }

    public Chunk GetLeftChunk()
    {
        return new ChunkPosition(chunkPosition.chunkX - 1, chunkPosition.dimension).GetChunk();
    }

    private void TrySpawnMobs()
    {
        var r = new Random();

        if (!(r.NextDouble() < mobSpawningChance / TickRate) || Entity.LivingEntityCount >= Entity.MaxLivingAmount) 
            return;
        
        var x = r.Next(0, Width) + chunkPosition.worldX;
        var y = GetTopmostBlock(x, chunkPosition.dimension, true).location.y + 1;
        var entities = MobSpawnTypes;
        entities.AddRange(GetBiome().biomeSpecificEntitySpawns);
        var entityType = entities[r.Next(0, entities.Count)];

        var entity = Entity.Spawn(entityType);
        entity.Location = new Location(x, y, chunkPosition.dimension);
    }

    private Dictionary<Location, Material> GenerateChunkTerrain()
    {
        var blockList = new Dictionary<Location, Material>();

        for (var y = 0; y <= Height; y++)
        for (var x = 0; x < Width; x++)
        {
            var loc = new Location(x + chunkPosition.worldX, y, chunkPosition.dimension);
            var mat = GenerateTerrainBlock(loc);

            if (mat != Material.Air) blockList.Add(loc, mat);
        }

        return blockList;
    }

    private IEnumerator GenerateChunk()
    {
        patchNoise = new Perlin(0.6f, 0.8f, 0.8f, 2, WorldManager.world.seed, QualityMode.Low);
        caveNoise = new Perlin(CaveFrequency, CaveLacunarity, CavePercistance, CaveOctaves,
            WorldManager.world.seed, QualityMode.High);

        isLoading = true;
        WorldManager.instance.amountOfChunksLoading++;

        //pre-generate chunk biomes
        Biome.GetBiomeAt(chunkPosition);


        if (chunkPosition.HasBeenGenerated())
        {
            //Load blocks
            var path = WorldManager.world.getPath() + "\\region\\" + chunkPosition.dimension + "\\" +
                       chunkPosition.chunkX + "\\blocks";
            if (File.Exists(path))
                foreach (var line in File.ReadAllLines(path))
                {
                    var loc = new Location(int.Parse(line.Split('*')[0].Split(',')[0]),
                        int.Parse(line.Split('*')[0].Split(',')[1]));
                    var mat = (Material) Enum.Parse(typeof(Material), line.Split('*')[1]);
                    var data = new BlockData(line.Split('*')[2]);

                    CreateLocalBlock(loc, mat, data);
                }

            //Loading Entities
            LoadAllEntities();
        }
        else
        {
            chunkPosition.CreateChunkPath();

            //Generate Terrain Blocks
            Dictionary<Location, Material> terrainBlocks = null;
            var terrainThread = new Thread(() => { terrainBlocks = GenerateChunkTerrain(); });
            terrainThread.Start();

            while (terrainThread.IsAlive) 
                yield return new WaitForSeconds(0.5f);

            var i = 0;
            foreach (var terrainBlock in terrainBlocks)
            {
                terrainBlock.Key.SetMaterial(terrainBlock.Value);

                i++;
                if (i % 10 == 1) yield return new WaitForSeconds(0.05f);
            }
            
            //Generate Structures
            for (var y = 0; y <= Height; y++)
            {
                for (var x = 0; x < Width; x++)
                    GenerateStructures(Location.LocationByPosition(transform.position, chunkPosition.dimension) +
                                       new Location(x, y));

                if (y < 80 && y % 4 == 0)
                    yield return new WaitForSeconds(0.05f);
            }

            //Generate Tick all block (decay all necessary grass etc)
            GeneratingTickAllBlocks();

            //Mark chunk as Generated
            var chunkDataPath = WorldManager.world.getPath() + "\\region\\" + chunkPosition.dimension + "\\" +
                                chunkPosition.chunkX + "\\chunk";
            var chunkDataLines = File.ReadAllLines(chunkDataPath).ToList();
            chunkDataLines.Add("hasBeenGenerated=true");
            File.WriteAllLines(chunkDataPath, chunkDataLines);
        }

        StartCoroutine(Tick());

        isLoading = false;
        isLoaded = true;
        WorldManager.instance.amountOfChunksLoading--;

        StartCoroutine(GenerateSunlightLoop());
        StartCoroutine(GenerateLight());
        StartCoroutine(ProcessLightLoop());
    }

    private IEnumerator GenerateSunlightLoop()
    {
        var lastUpdateTime = "none";

        while (true)
        {
            var isNight = WorldManager.world.time % WorldManager.dayLength > WorldManager.dayLength / 2;
            var currentTime = isNight ? "night" : "day";

            if (currentTime != lastUpdateTime)
            {
                //Fill sunlight source list
                var minXPos = chunkPosition.worldX;
                var maxXPos = chunkPosition.worldX + Width - 1;

                for (var x = minXPos; x <= maxXPos; x++) Block.UpdateSunlightSourceAt(x, chunkPosition.dimension);

                lastUpdateTime = currentTime;
            }

            yield return new WaitForSecondsRealtime(10f);
        }
    }

    private IEnumerator GenerateLight()
    {
        //Update Light Sources (not sunlight again)
        foreach (var block in GetComponentsInChildren<Block>())
            if (block.glowLevel > 0)
                Block.UpdateLightAround(block.location);
        yield return new WaitForSecondsRealtime(0.05f);
    }

    private IEnumerator ProcessLightLoop()
    {
        while (true)
        {
            if (lightSourceToUpdate.Count > 0)
            {
                var oldLightCopy = new List<Location>(lightSourceToUpdate);
                lightSourceToUpdate.Clear();
                List<KeyValuePair<Block, int>> lightToRender = null;

                var lightThread = new Thread(() => { lightToRender = processDirtyLight(oldLightCopy); });
                lightThread.Start();

                while (lightThread.IsAlive) yield return new WaitForSeconds(0.1f);

                //Render
                foreach (var entry in new List<KeyValuePair<Block, int>>(lightToRender))    //Not using dictionaries, since it doesn't work multi-threaded
                {
                    if (entry.Key == null)
                        continue;

                    entry.Key.RenderBlockLight(entry.Value);
                }
            }

            yield return new WaitForSecondsRealtime(0.2f);
        }
    }

    private List<KeyValuePair<Block, int>> processDirtyLight(List<Location> lightToProcess)
    {
        var lightToRender = new List<KeyValuePair<Block, int>>();

        if (lightToProcess.Count == 0)
            return lightToRender;

        //Process
        foreach (var loc in lightToProcess)
            for (var x = loc.x - 15; x < loc.x + 15; x++)
            for (var y = loc.y - 15; y < loc.y + 15 && y > 0 && y < Height; y++)
            {
                var blockLoc = new Location(x, y, loc.dimension);

                var block = blockLoc.GetBlock();

                if (block == null)
                    continue;

                var result = new KeyValuePair<Block, int>(block, Block.GetLightLevel(blockLoc));
                lightToRender.Add(result);
            }

        //Remove Copies
        lightToRender = lightToRender.Distinct().ToList();

        return lightToRender;
    }

    private void LoadAllEntities()
    {
        var path = WorldManager.world.getPath() + "\\region\\" + chunkPosition.dimension + "\\" + chunkPosition.chunkX +
                   "\\entities";

        if (!Directory.Exists(path))
            return;

        foreach (var entityPath in Directory.GetFiles(path))
        {
            var entityFile = entityPath.Split('\\')[entityPath.Split('\\').Length - 1];
            var entityType = entityFile.Split('.')[1];
            var entityId = int.Parse(entityFile.Split('.')[0]);

            var entity = Entity.Spawn(entityType);
            entity.id = entityId;
            //Make sure the newly created entity is in the chunk, to make loading work correctly (setting actual position happens inside Entity class)
            entity.transform.position = transform.position + new Vector3(1, 1);
        }
    }

    private void GenerateStructures(Location loc)
    {
        var block = loc.GetBlock();
        if (block == null)
            return;

        var mat = block.GetMaterial();
        var biome = GetBiome();
        var r = new Random(SeedGenerator.SeedByLocation(loc));
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
                if (r.Next(0, 100) <= 3)
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
    }

    private bool IsBlockLocal(Location loc)
    {
        var local = new ChunkPosition(loc).chunkX == chunkPosition.chunkX && loc.dimension == chunkPosition.dimension;

        if (loc.y < 0 || loc.y > Height || loc.dimension != chunkPosition.dimension)
            local = false;

        return local;
    }

    public Block CreateLocalBlock(Location loc, Material mat, BlockData data)
    {
        var pos = new int2(loc.GetPosition());

        var type = Type.GetType(mat.ToString());
        if (!type.IsSubclassOf(typeof(Block)))
            return null;

        if (!IsBlockLocal(loc))
        {
            Debug.LogWarning("Tried setting local block outside of chunk (" + loc.x + ", " + loc.y +
                             ") inside Chunk [" + chunkPosition.chunkX + ", " + chunkPosition.dimension +
                             "]");
            return null;
        }

        //remove old block
        if (GetLocalBlock(loc) != null)
        {
            Destroy(GetLocalBlock(loc).gameObject);
            blocks.Remove(pos);
        }

        Block result = null;

        if (mat != Material.Air)
        {
            //Place new block
            GameObject blockObject = null;

            blockObject = Instantiate(blockPrefab, transform, true);

            //Attach it to the object
            var block = (Block) blockObject.AddComponent(type);

            blockObject.transform.position = loc.GetPosition();

            //Add the block to block list
            if (blocks.ContainsKey(pos))
                blocks[pos] = block;
            else
                blocks.Add(pos, block);

            block.data = data;
            block.location = loc;
            block.ScheduleBlockInitialization();

            result = blockObject.GetComponent<Block>();
        }

        if (isLoaded)
        {
            Block.UpdateSunlightSourceAt(loc.x, Player.localInstance.Location.dimension);
            Block.UpdateLightAround(loc);
        }

        return result;
    }

    public Block GetLocalBlock(Location loc)
    {
        if (!IsBlockLocal(loc))
        {
            Debug.LogWarning("Tried getting local block outside of chunk (" + loc.x + ", " + loc.y +
                             ") inside Chunk [" + chunkPosition.chunkX + ", " + chunkPosition.dimension + "]");
            return null;
        }

        Block block;
        blocks.TryGetValue(new int2(loc.x, loc.y), out block);

        return block;
    }

    private Material GenerateTerrainBlock(Location loc)
    {
        var r = new Random(SeedGenerator.SeedByLocation(loc));
        var mat = Material.Air;

        float noiseValue;

        var biome = GetBiome();
        var rightBiome = Biome.GetBiomeAt(new ChunkPosition(chunkPosition.chunkX + 1, chunkPosition.dimension));
        var leftBiome = Biome.GetBiomeAt(new ChunkPosition(chunkPosition.chunkX - 1, chunkPosition.dimension));
        float primaryBiomeWeight;

        if (biome != rightBiome)
        {
            primaryBiomeWeight = 0.5f - (float)Mathf.Abs(loc.x - new ChunkPosition(loc).chunkX * Width) / Width / 2f;
            noiseValue = Biome.BlendNoiseValues(loc, biome, rightBiome, primaryBiomeWeight);
        }
        else if (biome != leftBiome)
        {
            primaryBiomeWeight = 0.5f + (float)Mathf.Abs(loc.x - new ChunkPosition(loc).chunkX * Width) / Width / 2f;
            noiseValue = Biome.BlendNoiseValues(loc, biome, leftBiome, primaryBiomeWeight);
        }
        else
        {
            noiseValue = biome.GetLandscapeNoiseAt(loc);
        }

        //-Terrain Generation-//

        //-Ground-//
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

            if (noiseValue > biome.stoneLayerNoiseValue) mat = Material.Stone;
        }

        //-Lakes-//
        if (mat == Material.Air && loc.y <= SeaLevel) mat = Material.Water;

        //-Dirt & Gravel Patches-//
        if (mat == Material.Stone)
        {
            if (Mathf.Abs((float) caveNoise.GetValue((float) loc.x / 20, (float) loc.y / 20)) > 7.5f)
                mat = Material.Dirt;
            if (Mathf.Abs((float) caveNoise.GetValue((float) loc.x / 20 + 100, (float) loc.y / 20, 200)) > 7.5f)
                mat = Material.Gravel;
        }

        //-Sea-//
        if (mat == Material.Air && loc.y <= SeaLevel) mat = Material.Water;

        //-Caves-//
        if (noiseValue > 0.1f)
        {
            var caveValue =
                (caveNoise.GetValue((float) loc.x / 20, (float) loc.y / 20) + 4.0f) / 4f;
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

    public Entity[] GetEntities()
    {
        var entities = new List<Entity>();

        foreach (var e in Entity.entities)
            if (e.Location.x >= chunkPosition.worldX &&
                e.Location.x <= chunkPosition.worldX + Width)
                entities.Add(e);

        return entities.ToArray();
    }

    public static Block GetTopmostBlock(int x, Dimension dimension, bool mustBeSolid)
    {
        var chunk = new ChunkPosition(new Location(x, 0, dimension)).GetChunk();
        if (chunk == null)
            return null;

        return chunk.GetLocalTopmostBlock(x, mustBeSolid);
    }

    public Block GetLocalTopmostBlock(int x, bool mustBeSolid)
    {
        for (var y = Height; y > 0; y--)
        {
            var block = GetLocalBlock(new Location(x, y, chunkPosition.dimension));

            if (block != null)
            {
                if (mustBeSolid && !block.playerCollide)
                    continue;

                return block;
            }
        }

        return null;
    }

    public Biome GetBiome()
    {
        return Biome.GetBiomeAt(chunkPosition);
    }
}