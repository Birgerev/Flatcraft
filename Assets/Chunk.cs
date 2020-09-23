using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibNoise;
using System.IO;
using System.Linq;
using System.Threading;
using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public class Chunk : MonoBehaviour
{
    public const int Width = 16, Height = 255;
    public const int RenderDistance = 4;
    public const int AmountOfChunksInRegion = 16;
    public const int SpawnChunkDistance = 0;
    public const int OutsideRenderDistanceUnloadTime = 10;
    public const int TickRate = 1;

    public GameObject blockPrefab;

    public ChunkPosition chunkPosition;
    public bool isSpawnChunk = false;
    public bool isLoaded = false;
    public bool isLoading = false;
    public int age = 0;
    
    public Dictionary<int2, Block> blocks = new Dictionary<int2, Block>();

    [Header("Cave Generation Settings")]
    private const float CaveFrequency = 5;
    private const float CaveLacunarity = 0.6f;
    private const float CavePercistance = 2;
    private const int CaveOctaves = 4;
    private const float CaveHollowValue = 2.2f;

    [Header("Ore Generation Settings")]
    private const int OreCoalHeight = 128;
    private const double OreCoalChance = 0.008f;

    private const int OreIronHeight = 64;
    private const double OreIronChance = 0.005f;

    private const int OreGoldHeight = 32;
    private const double OreGoldChance = 0.0015f;

    private const int OreLapisHeight = 32;
    private const double OreLapisChance = 0.0015f;

    private const int OreRedstoneHeight = 16;
    private const double OreRedstoneChance = 0.0015f;

    private const int OreDiamondHeight = 16;
    private const double OreDiamondChance = 0.0015f;

    private const int LavaHeight = 10;
    public const int SeaLevel = 62;
    
    private static float mobSpawningChance = 0.01f;
    private static List<string> mobSpawns = new List<string> { "Chicken", "Sheep" };

    public HashSet<Location> lightSourceToUpdate = new HashSet<Location>();

    private void Start()
    {
        isSpawnChunk = (chunkPosition.chunkX >= -SpawnChunkDistance && chunkPosition.chunkX <= SpawnChunkDistance);

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
        for(int i = -distance; i < distance; i++)
        {
            ChunkPosition cPos = new ChunkPosition((int)((float)loc.x / (float)Width) + i, loc.dimension);
            if(!cPos.IsChunkLoaded())
                cPos.CreateChunk();
        }
    }

    public void TickAllBlocks()
    {
        //Tick Blocks
        Block[] blocks = transform.GetComponentsInChildren<Block>();

        foreach (Block block in blocks)
        {
            if (block == null || block.transform == null)
                continue;

            block.Tick();
        }
    }

    IEnumerator AutosaveAllBlocks()
    {
        List<Block> blocks = new List<Block>(this.blocks.Values);

        if (blocks.Count > 0)
        {
            int blocksPerBatch = 20;
            float timePerBatch = 5f / ((float)blocks.Count / (float)blocksPerBatch);
            foreach (Block block in blocks)
            {
                yield return new WaitForSeconds(timePerBatch);
                if (block == null || !block.autosave)
                    continue;


                block.Tick();
                block.Autosave();
            }
        }

    }

    IEnumerator SelfDestructionChecker()
    {
        while (true)
        {
            float timePassedOutsideRenderDistance = 0f;
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

    IEnumerator Tick()
    {
        while (true)
        {
            //Update neighbor chunks
            if (age < 5)
            {
                TrySpawnMobs();
            }

            age++;
            yield return new WaitForSeconds(1 / TickRate);
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
    
    public void TrySpawnMobs()
    {
        System.Random r = new System.Random();

        if (r.NextDouble() < mobSpawningChance / TickRate && Entity.livingEntityCount < Entity.MaxLivingAmount)
        {
            int x = r.Next(0, Width) + chunkPosition.worldX;
            int y = getTopmostBlock(x, chunkPosition.dimension).location.y + 1;
            List<string> entities = mobSpawns;
            entities.AddRange(getBiome().biomeSpecificEntitySpawns);
            string entityType = entities[r.Next(0, entities.Count)];

            Entity entity = Entity.Spawn(entityType);
            entity.location = new Location(x, y, chunkPosition.dimension);
        }
    }

    private Dictionary<Location, Material> GenerateChunkTerrain()
    {
        Dictionary<Location, Material> blocks = new Dictionary<Location, Material>();

        for (int y = 0; y <= Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Location loc = new Location(x + chunkPosition.worldX, y, chunkPosition.dimension);
                Material mat = GenerateTerrainBlock(loc);

                if (mat != Material.Air)
                {
                    blocks.Add(loc, mat);
                }
            }
        }

        return blocks;
    }

    IEnumerator GenerateChunk()
    {
        patchNoise = new LibNoise.Generator.Perlin(0.6f, 0.8f, 0.8f, 2, WorldManager.world.seed, QualityMode.Low);
        caveNoise = new LibNoise.Generator.Perlin(CaveFrequency, CaveLacunarity, CavePercistance, CaveOctaves,
            WorldManager.world.seed, QualityMode.High);

        isLoading = true;
        WorldManager.instance.amountOfChunksLoading++;

        if (chunkPosition.HasBeenGenerated())
        {
            //Load blocks
            string path = WorldManager.world.getPath() + "\\region\\" + chunkPosition.dimension.ToString() + "\\" +
                          chunkPosition.chunkX + "\\blocks";
            if (File.Exists(path))
            {
                foreach (string line in File.ReadAllLines(path))
                {
                    Location loc = new Location(int.Parse(line.Split('*')[0].Split(',')[0]),
                        int.Parse(line.Split('*')[0].Split(',')[1]));
                    Material mat = (Material) System.Enum.Parse(typeof(Material), line.Split('*')[1]);
                    BlockData data = new BlockData(line.Split('*')[2]);
                    
                    CreateLocalBlock(loc, mat, data);
                }
            }
            
            //Loading Entities
            LoadAllEntities();
        }
        else
        {
            chunkPosition.CreateChunkPath();

            //Generate Terrain Blocks
            Dictionary<Location, Material> terrainBlocks = null;
            Thread terrainThread = new Thread(() => { terrainBlocks = GenerateChunkTerrain(); });
            terrainThread.Start();

            while (terrainThread.IsAlive)
            {
                yield return new WaitForSeconds(0.5f);
            }

            int i = 0;
            foreach (KeyValuePair<Location, Material> terrainBlock in terrainBlocks)
            {
                terrainBlock.Key.SetMaterial(terrainBlock.Value);
                
                i++;
                if (i % 10 == 1)
                {
                    yield return new WaitForSeconds(0.05f);
                }
            }
            
            //Generate Tick all block (decay all necessary grass etc)
            List<Block> blocksToTick = new List<Block>(blocks.Values);
            foreach(Block block in blocksToTick)
            {
                if(block != null)
                    block.GeneratingTick();
            }
            
            //Generate Structures
            for (int y = 0; y <= Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    GenerateStructures(Location.LocationByPosition(transform.position, chunkPosition.dimension) +
                                       new Location(x, y));
                }

                if (y < 80 && y % 4 == 0)
                    yield return new WaitForSeconds(0.05f);
            }
            
            //Mark chunk as Generated
            string chunkDataPath = WorldManager.world.getPath() + "\\region\\" + chunkPosition.dimension + "\\" + chunkPosition.chunkX + "\\chunk";
            List<string> chunkDataLines = File.ReadAllLines(chunkDataPath).ToList();
            chunkDataLines.Add("hasBeenGenerated=true");
            File.WriteAllLines(chunkDataPath, chunkDataLines);
            
            
            TickAllBlocks();
        }
        
        StartCoroutine(Tick());

        isLoading = false;
        isLoaded = true;
        WorldManager.instance.amountOfChunksLoading--;
        
        StartCoroutine(GenerateSunlightLoop());
        StartCoroutine(GenerateLight());
        StartCoroutine(ProcessLightLoop());
    }
    
    IEnumerator GenerateSunlightLoop()
    {
        string lastUpdateTime = "none";

        while (true)
        {
            bool isNight = (WorldManager.world.time % WorldManager.dayLength) > (WorldManager.dayLength / 2);
            string currentTime = isNight ? "night" : "day";

            if (currentTime != lastUpdateTime)
            {
                //Fill sunlight source list
                int minXPos = chunkPosition.worldX;
                int maxXPos = chunkPosition.worldX + Width - 1;

                for (int x = minXPos; x <= maxXPos; x++)
                {
                    Block.UpdateSunlightSourceAt(x, chunkPosition.dimension);
                }

                lastUpdateTime = currentTime;
            }

            yield return new WaitForSecondsRealtime(10f);
        }
    }

    IEnumerator GenerateLight()
    {
        //Update Light Sources (not sunlight again)
        foreach (Block block in GetComponentsInChildren<Block>())
        {
            if (block.glowLevel > 0)
            {
                Block.UpdateLightAround(block.location);
            }
        }
        yield return new WaitForSecondsRealtime(0.05f);
    }

    IEnumerator ProcessLightLoop()
    {
        while (true)
        {
            if (lightSourceToUpdate.Count > 0)
            {
                List<Location> oldLightCopy = new List<Location>(lightSourceToUpdate);
                lightSourceToUpdate.Clear();
                List<KeyValuePair<Block, int>> lightToRender = null;

                Thread lightThread = new Thread(() => { lightToRender = processDirtyLight(oldLightCopy); });
                lightThread.Start();

                while (lightThread.IsAlive)
                {
                    yield return new WaitForSeconds(0.1f);
                }

                //Render
                foreach (KeyValuePair<Block, int> entry in new List<KeyValuePair<Block, int>>(lightToRender))    //Not using dictionaries, since it doesn't work multithreaded
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
        List<KeyValuePair<Block, int>> lightToRender = new List<KeyValuePair<Block, int>>();

        if (lightToProcess.Count == 0)
            return lightToRender;
        
        //Process
        foreach (Location loc in lightToProcess)
        {
            for (int x = loc.x - 15; x < loc.x + 15; x++)
            {
                for (int y = loc.y - 15; y < loc.y + 15 && y > 0 && y < Chunk.Height; y++)
                {
                    Location blockLoc = new Location(x, y, loc.dimension);
                    
                    Block block = blockLoc.GetBlock();
                    
                    if (block == null)
                        continue;
                    
                    KeyValuePair<Block, int> result = new KeyValuePair<Block, int>(block, Block.GetLightLevel(blockLoc));
                    lightToRender.Add(result);
                }
            }
        }
        
        //Remove Copies
        lightToRender = lightToRender.Distinct().ToList();
        
        return lightToRender;
    }

    private void LoadAllEntities()
    {
        string path = WorldManager.world.getPath() + "\\region\\" + chunkPosition.dimension + "\\" + chunkPosition.chunkX + "\\entities";

        if (!Directory.Exists(path))
            return;

        foreach (string entityPath in Directory.GetFiles(path))
        {
            string entityFile = entityPath.Split('\\')[entityPath.Split('\\').Length - 1];
            string entityType = entityFile.Split('.')[1];
            int entityId = int.Parse(entityFile.Split('.')[0]);

            Entity entity = Entity.Spawn(entityType);
            entity.id = entityId;
            //Make sure the newly created entity is in the chunk, to make loading work correctly (setting actual position happens inside Entity class)
            entity.transform.position = transform.position + new Vector3(1, 1);
        }
    }

    private void GenerateStructures(Location loc)
    {
        Block block = loc.GetBlock();
        if (block == null)
            return;

        Material mat = block.GetMaterial();
        Biome biome = getBiome();
        System.Random r = new System.Random(SeedGenerator.SeedByLocation(loc));
        Material[] flowerMaterials = new Material[] { Material.Red_Flower };
        Material[] vegetationMaterials = new Material[] { Material.Tall_Grass};
        
        if ((mat == Material.Grass || mat == Material.Sand) && (loc + new Location(0, 1)).GetBlock() == null)
        {
            //Vegetation
            if(biome.name == "forest" || biome.name == "forest_hills" || biome.name == "birch_forest" || biome.name == "plains")
                if (r.Next(0, 100) <= 50)
                    (loc + new Location(0, 1)).SetMaterial(vegetationMaterials[r.Next(0, vegetationMaterials.Length)]);
            
            if(biome.name == "forest" || biome.name == "forest_hills" || biome.name == "birch_forest" || biome.name == "plains")
                if (r.Next(0, 100) <= 10)
                    (loc + new Location(0, 1)).SetMaterial(flowerMaterials[r.Next(0, flowerMaterials.Length)]);
                
            //Trees
            if(biome.name == "forest" || biome.name == "forest_hills")
                 if (r.Next(0, 100) <= 20)
                     (loc + new Location(0, 1)).SetMaterial(Material.Structure_Block).SetData(new BlockData("structure=Oak_Tree"));
            
            //Birch Trees
            if(biome.name == "birch_forest")
                if (r.Next(0, 100) <= 20)
                    (loc + new Location(0, 1)).SetMaterial(Material.Structure_Block).SetData(new BlockData("structure=Birch_Tree"));
            
            //Unlikely Trees
            if(biome.name == "plains")
                if (r.Next(0, 100) <= 3)
                    (loc + new Location(0, 1)).SetMaterial(Material.Structure_Block).SetData(new BlockData("structure=Oak_Tree"));
            
            //Large Trees
            if(biome.name == "plains")
                if (r.Next(0, 100) <= 3)
                    (loc + new Location(0, 1)).SetMaterial(Material.Structure_Block).SetData(new BlockData("structure=Large_Oak_Tree"));
            
            //Dead Bushes
            if (biome.name == "desert")
                if (r.Next(0, 100) <= 8)
                    (loc + new Location(0, 1)).SetMaterial(Material.Dead_Bush);
                
            //Cactie
            if (biome.name == "desert")
                if (r.Next(0, 100) <= 5)
                    (loc + new Location(0, 1)).SetMaterial(Material.Structure_Block).SetData(new BlockData("structure=Cactus"));
        }
        
        
        //Generate Ores
        if (mat == Material.Stone)
        {
            if (r.NextDouble() < Chunk.OreDiamondChance && loc.y <= Chunk.OreDiamondHeight)
            {
                (loc + new Location(0, 1)).SetMaterial(Material.Structure_Block).SetData(new BlockData("structure=Ore_Diamond"));
            }
            else if (r.NextDouble() < Chunk.OreRedstoneChance && loc.y <= Chunk.OreRedstoneHeight)
            {
                (loc + new Location(0, 1)).SetMaterial(Material.Structure_Block).SetData(new BlockData("structure=Ore_Redstone"));
            }
            else if (r.NextDouble() < Chunk.OreLapisChance && loc.y <= Chunk.OreLapisHeight)
            {
                (loc + new Location(0, 1)).SetMaterial(Material.Structure_Block).SetData(new BlockData("structure=Ore_Lapis"));
            }
            else if (r.NextDouble() < Chunk.OreGoldChance && loc.y <= Chunk.OreGoldHeight)
            {
                (loc + new Location(0, 1)).SetMaterial(Material.Structure_Block).SetData(new BlockData("structure=Ore_Gold"));
            }
            else if (r.NextDouble() < Chunk.OreIronChance && loc.y <= Chunk.OreIronHeight)
            {
                (loc + new Location(0, 1)).SetMaterial(Material.Structure_Block).SetData(new BlockData("structure=Ore_Iron"));
            }
            else if (r.NextDouble() < Chunk.OreCoalChance && loc.y <= Chunk.OreCoalHeight)
            {
                (loc + new Location(0, 1)).SetMaterial(Material.Structure_Block).SetData(new BlockData("structure=Ore_Coal"));
            }
        }
    }

    public bool isBlockLocal(Location loc)
    {
        bool local = (new ChunkPosition(loc).chunkX == chunkPosition.chunkX && loc.dimension == chunkPosition.dimension);
        
        if (loc.y < 0 || loc.y > Height || loc.dimension != chunkPosition.dimension)
            local = false;

        return local;
    }

    public Block CreateLocalBlock(Location loc, Material mat, BlockData data)
    {
        int2 pos = new int2(loc.GetPosition());

        System.Type type = System.Type.GetType(mat.ToString());
        if (!type.IsSubclassOf(typeof(Block)))
            return null;

        if (!isBlockLocal(loc))
        {
            Debug.LogWarning("Tried setting local block outside of chunk (" + loc.x + ", " + loc.y +
                             ") inside Chunk [" + chunkPosition.chunkX + ", " + chunkPosition.dimension.ToString() +
                             "]");
            return null;
        }

        //remove old block
        if (getLocalBlock(loc) != null)
        {
            Destroy(getLocalBlock(loc).gameObject);
            blocks.Remove(pos);
        }

        Block result = null;

        if (mat != Material.Air)
        {
            //Place new block
            GameObject blockObject = null;

            blockObject = Instantiate(blockPrefab, transform, true);

            //Attach it to the object
            Block block = (Block)blockObject.AddComponent(type);

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
            Block.UpdateSunlightSourceAt(loc.x, Player.localInstance.location.dimension);
            Block.UpdateLightAround(loc);
        }

        return result;
    }
    
    public Block getLocalBlock(Location loc)
    {
        if (!isBlockLocal(loc))
        {
            Debug.LogWarning("Tried getting local block outside of chunk (" + loc.x + ", " + loc.y + ") inside Chunk [" + chunkPosition.chunkX + ", " + chunkPosition.dimension.ToString() + "]");
            return null;
        }
        
        Block block = null;
        
        blocks.TryGetValue(new int2(loc.x, loc.y), out block);
        
        return block;
    }
    
    LibNoise.Generator.Perlin caveNoise;
    LibNoise.Generator.Perlin patchNoise;
    private Material GenerateTerrainBlock(Location loc)
    {
        System.Random r = new System.Random(SeedGenerator.SeedByLocation(loc));
        Material mat = Material.Air;

        float noiseValue;
        
        Biome biome = getBiome();
        Biome rightBiome = Biome.getBiomeAt(new ChunkPosition(chunkPosition.chunkX + 1, chunkPosition.dimension));
        Biome leftBiome = Biome.getBiomeAt(new ChunkPosition(chunkPosition.chunkX - 1, chunkPosition.dimension));
        float primaryBiomeWeight;
        
        if (biome != rightBiome)
        {
            primaryBiomeWeight = 0.5f - ((Mathf.Abs(loc.x - (new ChunkPosition(loc).chunkX * Width)) / Width) / 2);
            noiseValue = Biome.blendNoiseValues(loc, biome, rightBiome, primaryBiomeWeight);
        }
        else if (biome != leftBiome)
        {
            primaryBiomeWeight = 0.5f + ((Mathf.Abs(loc.x - (new ChunkPosition(loc).chunkX * Width)) / Width) / 2);
            noiseValue = Biome.blendNoiseValues(loc, biome, leftBiome, primaryBiomeWeight);
        }
        else
        {
            noiseValue = biome.getLandscapeNoiseAt(loc);
        }

        //-Terrain Generation-//
        
        //-Ground-//
        if (noiseValue > 0.1f)
        {
            if (biome.name == "desert")
            {
                mat = Material.Sand;
                if(noiseValue > biome.stoneLayerNoiseValue - 2)
                    mat = Material.Sandstone;
            }
            else if (biome.name == "forest" || biome.name == "forest_hills" || biome.name == "birch_forest" || biome.name == "plains")
            {
                mat = Material.Grass;
            }

            if (noiseValue > biome.stoneLayerNoiseValue)
            {
                mat = Material.Stone;
            }
        }
        
        //-Lakes-//
        if (mat == Material.Air && loc.y <= SeaLevel)
        {
            mat = Material.Water;
        }

        //-Dirt & Gravel Patches-//
        if (mat == Material.Stone)
        {
            if (Mathf.Abs((float)caveNoise.GetValue((float)loc.x / 20, (float)loc.y / 20)) > 7.5f)
            {
                mat = Material.Dirt;
            }
            if (Mathf.Abs((float)caveNoise.GetValue((float)loc.x / 20 + 100, (float)loc.y / 20, 200)) > 7.5f)
            {
                mat = Material.Gravel;
            }
        }

        //-Sea-//
        if (mat == Material.Air && loc.y <= SeaLevel)
        {
            mat = Material.Water;
        }

        //-Caves-//
        if(noiseValue > 0.1f)
        {
            double caveValue =
                (caveNoise.GetValue((float)loc.x / 20, (float)loc.y / 20) + 4.0f) / 4f;
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
            else if (r.Next(0, (int)loc.y+2) <= 1)
                mat = Material.Bedrock;
        }

        return mat;
    }

    public Entity[] GetEntities()
    {
        List<Entity> entities = new List<Entity>();
        
        foreach (Entity e in Entity.entities)
        {
            if (e.location.x >= chunkPosition.worldX && 
                e.location.x <= chunkPosition.worldX + Width)
                entities.Add(e);
        }

        return entities.ToArray();
    }

    public static Block getTopmostBlock(int x, Dimension dimension)
    {
        Chunk chunk = new ChunkPosition(new Location(x, 0, dimension)).GetChunk();
        if (chunk == null)
            return null;

        return chunk.getLocalTopmostBlock(x);
    }

    public Block getLocalTopmostBlock(int x)
    {
        for(int y = Height; y > 0; y--)
        {
            if(getLocalBlock(new Location(x, y, chunkPosition.dimension)) != null)
            {
                return getLocalBlock(new Location(x, y, chunkPosition.dimension));
            }
        }
        return null;
    }

    public Biome getBiome()
    {
        return Biome.getBiomeAt(chunkPosition);
    }
}