using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibNoise;
using System.IO;
using System.Linq;
using System.Threading;
using Unity.Burst;

[BurstCompile]
public class Chunk : MonoBehaviour
{
    public static float AutosaveDuration = 5;
    public const int Width = 16, Height = 255;
    public const int RenderDistance = 4;
    public const int AmountOfChunksInRegion = 16;
    public const int SpawnChunkDistance = 0;
    public const int MinimumUnloadTime = 20;
    public const int TickRate = 1;

    public GameObject blockPrefab;

    public ChunkPosition chunkPosition;
    public bool isSpawnChunk = false;
    public bool isTickedChunk = false;
    public bool isLoaded = false;
    public bool isLoading = false;
    public int age = 0;

    public Chunk rightChunk;
    public Chunk leftChunk;
    public Dictionary<Vector2Int, Block> blocks = new Dictionary<Vector2Int, Block>();
    public Dictionary<Location, int> cachedRandomSeeds = new Dictionary<Location, int>();

    [Header("Cave Generation Settings")]
    const float CaveFrequency = 5;
    const float CaveLacunarity = 0.6f;
    const float CavePercistance = 2;
    const int CaveOctaves = 4;
    const float CaveHollowValue = 2.2f;

    [Header("Ore Generation Settings")]
    const int OreCoalHeight = 128;
    const double OreCoalChance = 0.008f;

    const int OreIronHeight = 64;
    const double OreIronChance = 0.005f;

    const int OreGoldHeight = 32;
    const double OreGoldChance = 0.0015f;

    const int OreLapisHeight = 32;
    const double OreLapisChance = 0.0015f;

    const int OreRedstoneHeight = 16;
    const double OreRedstoneChance = 0.0015f;

    const int OreDiamondHeight = 16;
    const double OreDiamondChance = 0.0015f;

    const int LavaHeight = 10;
    public const int SeaLevel = 62;


    public static float mobSpawningChance = 0.01f;
    public static List<string> mobSpawns = new List<string> { "Chicken", "Sheep" };

    public HashSet<Location> lightSourceToUpdate = new HashSet<Location>();

    public Dictionary<Location, string> blockChanges = new Dictionary<Location, string>();

    private void Start()
    {
        if (WorldManager.instance.chunks.ContainsKey(chunkPosition))
        {
            Debug.LogWarning("A duplicate of Chunk [" + chunkPosition.chunkX + "] has been destroyed.");
            Destroy(gameObject);
            return;
        }

        isSpawnChunk = (chunkPosition.chunkX >= -SpawnChunkDistance && chunkPosition.chunkX <= SpawnChunkDistance);

        WorldManager.instance.chunks.Add(chunkPosition, this);

        StartCoroutine(SelfDestructionChecker());

        gameObject.name = "Chunk [" + chunkPosition.chunkX + "]";
        transform.position = new Vector3(chunkPosition.worldX, 0, 0);


        StartCoroutine(GenerateChunk());
    }

    private void OnDestroy()
    {
        if (WorldManager.instance.chunks.ContainsKey(chunkPosition) && (isLoaded || isLoading))
            WorldManager.instance.chunks.Remove(chunkPosition);
    }

    public static Chunk GetChunk(ChunkPosition pos)
    {
        return GetChunk(pos, true);
    }

    public static Chunk GetChunk(ChunkPosition pos, bool loadIfNotFound)
    {
        Chunk chunk = null;

        WorldManager.instance.chunks.TryGetValue(pos, out chunk);
        if (loadIfNotFound && chunk == null && pos.chunkX != 0)
        {
            chunk = LoadChunk(pos);
        }

        return chunk;
    }

    public static Chunk LoadChunk(ChunkPosition cPos)
    {
        GameObject newChunk = Instantiate(WorldManager.instance.chunkPrefab);

        newChunk.GetComponent<Chunk>().chunkPosition = cPos;

        return newChunk.GetComponent<Chunk>();
    }
    
    public void UnloadChunk()
    {
        if (isSpawnChunk)
            return;

        WorldManager.instance.chunks.Remove(chunkPosition);

        if (isLoading)
            WorldManager.instance.amountOfChunksLoading--;

        Destroy(gameObject);
    }

    IEnumerator TickAllBlocks()
    {
        //Tick Blocks
        Block[] blocks = transform.GetComponentsInChildren<Block>();

        foreach (Block block in blocks)
        {
            if (block == null || block.transform == null)
                continue;

            if (age == 0)
                block.GeneratingTick();

            block.Tick(false);
        }
        yield return new WaitForSecondsRealtime(0f);
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


                block.Tick(false);
                block.Autosave();
            }
        }

    }

    IEnumerator SelfDestructionChecker()
    {
        while (true)
        {
            float timePassed = 0f;
            while (!inRenderDistance())
            {
                yield return new WaitForSeconds(1f);
                timePassed += 1f;
                if (timePassed > MinimumUnloadTime)
                {
                    UnloadChunk();
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
            if (inRenderDistance())
            {
                if (rightChunk == null && inRenderDistance(new ChunkPosition(chunkPosition.chunkX + 1, chunkPosition.dimension)))
                {
                    rightChunk = GetChunk(new ChunkPosition(chunkPosition.chunkX + 1, chunkPosition.dimension));
                }
                if (leftChunk == null && inRenderDistance(new ChunkPosition(chunkPosition.chunkX - 1, chunkPosition.dimension)))
                {
                    leftChunk = GetChunk(new ChunkPosition(chunkPosition.chunkX - 1, chunkPosition.dimension));
                }
            }

            isTickedChunk = inRenderDistance(chunkPosition, RenderDistance - 1);

            //Update neighbor chunks
            if (isTickedChunk || age < 5)
            {
                TrySpawnMobs();
            }

            age++;
            yield return new WaitForSeconds(1 / TickRate);
        }
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

    public bool inRenderDistance()
    {
        return inRenderDistance(chunkPosition);
    }

    public bool inRenderDistance(ChunkPosition cPos)
    {
        return inRenderDistance(cPos, RenderDistance);
    }

    public bool inRenderDistance(ChunkPosition cPos, int range)
    {
        if (cPos.chunkX == 0)
            return true;

        Location playerLocation;


        if (Player.localInstance == null)
            playerLocation = new Location(0, 0);
        else
            playerLocation = Player.localInstance.location;

        float distanceFromPlayer = Mathf.Abs((cPos.worldX + (Width/2)) - playerLocation.x);

        return distanceFromPlayer < range * Width;
    }

    public Dictionary<Location, Material> loadChunkTerrain()
    {
        cacheRandomSeeds();
        Dictionary<Location, Material> blocks = new Dictionary<Location, Material>();

        for (int y = 0; y <= Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Location loc = new Location(x + chunkPosition.worldX, y, chunkPosition.dimension);
                Material mat = GetTheoreticalTerrainBlock(loc);

                if (mat != Material.Air)
                {
                    blocks.Add(loc, mat);
                }
            }
        }

        return blocks;
    }

    public void cacheRandomSeeds()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y <= Height; y++)
            {
                seedByLocation(new Location(x + chunkPosition.worldX, y));
            }
        }
    }

    IEnumerator GenerateChunk()
    {
        //Wait for scene to load
        yield return new WaitForSeconds(0.2f);

        patchNoise = new LibNoise.Generator.Perlin(0.6f, 0.8f, 0.8f, 2, WorldManager.world.seed, QualityMode.Low);
        caveNoise = new LibNoise.Generator.Perlin(CaveFrequency, CaveLacunarity, CavePercistance, CaveOctaves, WorldManager.world.seed, QualityMode.High);
        
        isLoading = true;
        WorldManager.instance.amountOfChunksLoading++;
        
        Dictionary<Location, Material> terrainBlocks = null;
        Thread terrainThread = new Thread(() => { terrainBlocks = loadChunkTerrain(); });
        terrainThread.Start();
        
        while (terrainThread.IsAlive)
        {
            yield return new WaitForSeconds(0.5f);
        }

        int i = 0;
        foreach (KeyValuePair<Location, Material> entry in terrainBlocks)
        {
            setLocalBlock(entry.Key, entry.Value, "", false, false);
            i++;
            if (i % 10 == 1)
            {
                yield return new WaitForSeconds(0.05f);
            }
        }  
        

        for (int y = 0; y <= Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                GenerateStructures(Location.locationByPosition(transform.position, chunkPosition.dimension) + new Location(x, y));
            }
            if (y < 80 && y % 4 == 0)
                yield return new WaitForSeconds(0.05f);
        }
        

        loadAllEntities();
        
        StartCoroutine(Tick());
        StartCoroutine(TickAllBlocks());
        yield return new WaitForSecondsRealtime(1f);
        

        //Load block changes
        string path = WorldManager.world.getPath() + "\\region\\" + chunkPosition.dimension.ToString() + "\\" + chunkPosition.chunkX + "\\blocks";
        if (File.Exists(path))
        {
            foreach (string line in File.ReadAllLines(path))
            {
                Location loc = new Location(int.Parse(line.Split('*')[0].Split(',')[0]), int.Parse(line.Split('*')[0].Split(',')[1]));
                Material mat = (Material)System.Enum.Parse(typeof(Material), line.Split('*')[1]);
                string data = line.Split('*')[2];

                setLocalBlock(loc, mat, data, false, false);
            }
        }

        isLoading = false;
        isLoaded = true;
        WorldManager.instance.amountOfChunksLoading--;
        
        StartCoroutine(GenerateSunlightLoop());
        StartCoroutine(GenerateLight());
        StartCoroutine(SaveLoop());
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
                    
                    Block block = Chunk.getBlock(blockLoc);
                    
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

    private void loadAllEntities()
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
        Block block = getBlock(loc);
        if (block == null)
            return;

        Material mat = block.GetMaterial();
        Biome biome = getBiome();
        System.Random r = new System.Random(Chunk.seedByLocation(loc));
        Material[] flowerMaterials = new Material[] { Material.Red_Flower };
        Material[] vegetationMaterials = new Material[] { Material.Tall_Grass};
        
        if ((mat == Material.Grass || mat == Material.Sand) && getBlock((loc + new Location(0, 1))) == null)
        {
            //Vegetation
            if(biome.name == "forest" || biome.name == "forest_hills" || biome.name == "birch_forest" || biome.name == "plains")
                if (r.Next(0, 100) <= 50)
                    Chunk.setBlock(loc + new Location(0, 1), vegetationMaterials[r.Next(0, vegetationMaterials.Length)], "", false, false);
            
            if(biome.name == "forest" || biome.name == "forest_hills" || biome.name == "birch_forest" || biome.name == "plains")
                if (r.Next(0, 100) <= 10)
                    Chunk.setBlock(loc + new Location(0, 1), flowerMaterials[r.Next(0, flowerMaterials.Length)], "", false, false);
                
            //Trees
            if(biome.name == "forest" || biome.name == "forest_hills")
                 if (r.Next(0, 100) <= 20)
                     Chunk.setBlock(loc + new Location(0, 1), Material.Structure_Block, "structure=Oak_Tree|save=false", false, false);
            
            //Birch Trees
            if(biome.name == "birch_forest")
                if (r.Next(0, 100) <= 20)
                    Chunk.setBlock(loc + new Location(0, 1), Material.Structure_Block, "structure=Birch_Tree|save=false", false, false);
            
            //Unlikely Trees
            if(biome.name == "plains")
                if (r.Next(0, 100) <= 3)
                    Chunk.setBlock(loc + new Location(0, 1), Material.Structure_Block, "structure=Oak_Tree|save=false", false, false);
            
            //Large Trees
            if(biome.name == "plains")
                if (r.Next(0, 100) <= 3)
                    Chunk.setBlock(loc + new Location(0, 1), Material.Structure_Block, "structure=Large_Oak_Tree|save=false", false, false);
            
            //Dead Bushes
            if (biome.name == "desert")
                if (r.Next(0, 100) <= 8)
                    Chunk.setBlock(loc + new Location(0, 1), Material.Dead_Bush, "", false, false);
                
            //Cactie
            if (biome.name == "desert")
                if (r.Next(0, 100) <= 5)
                    Chunk.setBlock(loc + new Location(0, 1), Material.Structure_Block, "structure=Cactus|save=false", false, false);
        }
        
        
        //Generate Ores
        if (mat == Material.Stone)
        {
            if (r.NextDouble() < Chunk.OreDiamondChance && loc.y <= Chunk.OreDiamondHeight)
            {
                Chunk.setBlock(loc, Material.Structure_Block, "structure=Ore_Diamond|save=false", false, false);
            }
            else if (r.NextDouble() < Chunk.OreRedstoneChance && loc.y <= Chunk.OreRedstoneHeight)
            {
                Chunk.setBlock(loc, Material.Structure_Block, "structure=Ore_Redstone|save=false", false, false);
            }
            else if (r.NextDouble() < Chunk.OreLapisChance && loc.y <= Chunk.OreLapisHeight)
            {
                Chunk.setBlock(loc, Material.Structure_Block, "structure=Ore_Lapis|save=false", false, false);
            }
            else if (r.NextDouble() < Chunk.OreGoldChance && loc.y <= Chunk.OreGoldHeight)
            {
                Chunk.setBlock(loc, Material.Structure_Block, "structure=Ore_Gold|save=false", false, false);
            }
            else if (r.NextDouble() < Chunk.OreIronChance && loc.y <= Chunk.OreIronHeight)
            {
                Chunk.setBlock(loc, Material.Structure_Block, "structure=Ore_Iron|save=false", false, false);
            }
            else if (r.NextDouble() < Chunk.OreCoalChance && loc.y <= Chunk.OreCoalHeight)
            {
                Chunk.setBlock(loc, Material.Structure_Block, "structure=Ore_Coal|save=false", false, false);
            }
        }
    }

    IEnumerator SaveLoop()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(AutosaveDuration);

            CreateChunkPath();

            StartCoroutine(AutosaveAllBlocks());


            //Save Block Changes
            if (blockChanges.Count > 0)
            {
                Thread worldThread = new Thread(SaveBlockChanges);
                worldThread.Start();

                while (worldThread.IsAlive)
                {
                    yield return new WaitForSeconds(0.1f);
                }
            }

            //Save Entities
            Entity[] entities = GetEntities();
            Thread entityThread = new Thread(() => { SaveEntities(entities); });
            entityThread.Start();

            while (entityThread.IsAlive)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
    }


    public void SaveEntities(Entity[] entities)
    {
        foreach (Entity e in entities)
        {
            try
            {
                e.Save();
            } catch (System.Exception ex)
            {
                Debug.LogWarning("Error in saving entity: " + ex.StackTrace);
            }
        }
    }

    public void CreateChunkPath()
    {
        string path = WorldManager.world.getPath() + "\\region\\" + chunkPosition.dimension + "\\" + chunkPosition.chunkX;
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(path + "\\entities");
            File.Create(path + "\\blocks").Close();
        }
    }

    public void SaveBlockChanges()
    {
        //Save Blocks
        if (blockChanges.Count > 0)
        {
            lock (blockChanges)
            {
                Dictionary<Location, string> changes = new Dictionary<Location, string>(blockChanges);
                blockChanges.Clear();

                string path = WorldManager.world.getPath() + "\\region\\" + chunkPosition.dimension + "\\" + chunkPosition.chunkX;
                foreach (string line in File.ReadAllLines(path + "\\blocks"))
                {
                    Location lineLoc = new Location(int.Parse(line.Split('*')[0].Split(',')[0]), int.Parse(line.Split('*')[0].Split(',')[1]));
                    string lineData = line.Split('*')[1] + "*" + line.Split('*')[2];

                    if (!changes.ContainsKey(lineLoc))
                        changes.Add(lineLoc, lineData);
                }

                //Empty lines before writing
                File.WriteAllText(path + "\\blocks", "");

                TextWriter c = new StreamWriter(path + "\\blocks");

                foreach (KeyValuePair<Location, string> line in changes)
                {
                    c.WriteLine(line.Key.x + "," + line.Key.y + "*" + line.Value);
                }

                c.Close();
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

    public static Block setBlock(Location loc, Material mat)
    {
        return setBlock(loc, mat, true);
    }

    public static Block setBlock(Location loc, Material mat, bool save)
    {
        return setBlock(loc, mat, "", save, true);
    }

    public static Block setBlock(Location loc, Material mat, string data, bool save, bool spreadTick)
    {
        Chunk chunk = GetChunk(new ChunkPosition(loc), false);

        if (chunk == null)
            return null;

        return chunk.setLocalBlock(loc, mat, data, save, spreadTick);
    }

    public Block setLocalBlock(Location loc, Material mat)
    {
        return setLocalBlock(loc, mat, true);
    }

    public Block setLocalBlock(Location loc, Material mat, bool save)
    {
        return setLocalBlock(loc, mat, "", save, true);
    }

    public Block setLocalBlock(Location loc, Material mat, string data, bool save, bool spreadTick)
    {
        Vector2Int pos = Vector2Int.RoundToInt(loc.getPosition());

        System.Type type = System.Type.GetType(mat.ToString());
        if (!type.IsSubclassOf(typeof(Block)))
            return null;

        if (!isBlockLocal(loc))
        {
            Debug.LogWarning("Tried setting local block outside of chunk (" + loc.x + ", " + loc.y + ") inside Chunk [" + chunkPosition.chunkX + ", " + chunkPosition.dimension.ToString() + "]");
            return null;
        }

        //remove old block
        if (getLocalBlock(loc) != null)
        {
            Destroy(getLocalBlock(loc).gameObject);
            blocks.Remove(pos);
        }
        
        if (save)
        {
            SaveBlock(loc, mat, data);
        }

        Block result = null;

        if (mat == Material.Air)
        {
            Block.SpreadTick(loc);
        }else
        {
            //Place new block
            GameObject block = null;

            block = Instantiate(blockPrefab);

            //Attach it to the object
            block.AddComponent(type);

            block.transform.parent = transform;
            block.transform.position = loc.getPosition();

            //Add the block to block list
            if(blocks.ContainsKey(pos))
                blocks[pos] = block.GetComponent<Block>();
            else
                blocks.Add(pos, block.GetComponent<Block>());

            block.GetComponent<Block>().data = Block.dataFromString(data);
            block.GetComponent<Block>().location = loc;
            block.GetComponent<Block>().Initialize();
            if (spreadTick)
                block.GetComponent<Block>().FirstTick();
            block.GetComponent<Block>().Tick(spreadTick);   ///TICKED BEFORE OTHER BLOCKS            

            result = block.GetComponent<Block>();
        }

        if (isLoaded)
        {
            Block.UpdateSunlightSourceAt(loc.x, Player.localInstance.location.dimension);
            Block.UpdateLightAround(loc);
        }
        return result;
    }

    public void SaveBlock(Block block)
    {
        SaveBlock(block.location, block.GetMaterial(), Block.stringFromData(block.data));
    }

    public void SaveBlock(Location loc, Material mat, string data)
    {
        if (blockChanges.ContainsKey(loc))
            blockChanges.Remove(loc);
        blockChanges.Add(loc, mat.ToString() + "*" + data);
    }
    
    public static Block getBlock(Location loc)
    {
        Chunk chunk = GetChunk(new ChunkPosition(loc), false);

        if (chunk == null)
        {
            return null;
        }

        Block block = chunk.getLocalBlock(loc);

        return block;
    }

    public Block getLocalBlock(Location loc)
    {
        if (!isBlockLocal(loc))
        {
            Debug.LogWarning("Tried getting local block outside of chunk (" + loc.x + ", " + loc.y + ") inside Chunk [" + chunkPosition.chunkX + ", " + chunkPosition.dimension.ToString() + "]");
            return null;
        }
        
        Block block = null;
        
        blocks.TryGetValue(Vector2Int.RoundToInt(loc.getPosition()), out block);
        
        return block;
    }
    
    LibNoise.Generator.Perlin caveNoise;
    LibNoise.Generator.Perlin patchNoise;
    public Material GetTheoreticalTerrainBlock(Location loc)
    {
        System.Random r = new System.Random(seedByLocation(loc));
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
        Chunk chunk = GetChunk(new ChunkPosition(new Location(x, 0, dimension)), false);
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

    public static int seedByLocation(Location loc)
    {
        Chunk chunk = Chunk.GetChunk(new ChunkPosition(loc), false);
        int seed = 0;

        if (chunk == null)
            return seed;

        chunk.cachedRandomSeeds.TryGetValue(loc, out seed);
        if (seed == 0) {
            seed = new System.Random((WorldManager.world.seed.ToString() + ", " + loc.x + ", " + loc.y + ", " + loc.dimension.ToString()).GetHashCode()).Next(int.MinValue, int.MaxValue);
            chunk.cachedRandomSeeds[loc] = seed;
        }

        return seed;
    }

    public Biome getBiome()
    {
        return Biome.getBiomeAt(chunkPosition);
    }
}