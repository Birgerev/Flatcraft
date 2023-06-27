using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Mirror;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using Random = System.Random;

[BurstCompile]
public class Chunk : NetworkBehaviour
{
    public const int Width = 16, Height = 256;
    public const int DimensionSeparationSpace = 512;
    public const int RenderDistance = 7;
    public const int AmountOfChunksInRegion = 16;
    public const int OutsideRenderDistanceUnloadTime = 10;
    public const int TickRate = 1;

    private static readonly float animalGenerationChance = 0.1f;
    private static readonly List<string> DefaultOverworldAnimals = new List<string> {"Chicken", "Sheep", "Cow", "Pig"};
    
    private static readonly int monsterSpawningLightLevel = 7;
    private static readonly int monsterSpawnAmountCap = 1;
    private static readonly List<string> DefaultOverworldMonsters = new List<string> {"Zombie", "Creeper", "Spider", "Skeleton", "Enderman"};

    public GameObject blockPrefab;
    public GameObject backgroundBlockPrefab;
    public List<Block> randomTickBlocks = new List<Block>();

    [SyncVar] public bool areBlocksReady;
    public bool donePlacingGeneratedBlocks;
    public bool donePlacingBackgroundBlocks;
    public bool isLoaded;
    public bool isLightGenerated;
    public bool startedInitializingAllBlocks;

    public Portal_Frame netherPortal;
    public Dictionary<int2, BackgroundBlock> backgroundBlocks = new Dictionary<int2, BackgroundBlock>();
    public Dictionary<int2, Block> blocks = new Dictionary<int2, Block>();

    public readonly SyncList<BlockState> blockStates = new SyncList<BlockState>();

    [SyncVar] public ChunkPosition chunkPosition;

    public WorldGenerator worldGenerator;


    private void Start()
    {
        if (isServer)
            WorldManager.instance.chunks.Add(chunkPosition, this);

        Debug.Log("Chunk [" + chunkPosition.chunkX + ", " + chunkPosition.dimension + "] has been created");

        StartCoroutine(CreateChunk());
    }

    public void OnDestroy()
    {
        WorldManager.instance.chunks.Remove(chunkPosition);
    }

    private IEnumerator CreateChunk()
    {
        WorldManager.instance.chunks[chunkPosition] = this;

        gameObject.name = "Chunk [" + chunkPosition.chunkX + " " + chunkPosition.dimension + "]";
        transform.position = new Location(chunkPosition.worldX, 0, chunkPosition.dimension).GetPosition();
        
        bool hasPreviouslyBeenGenerated = chunkPosition.HasEverBeenGenerated();

        if (isServer)
            areBlocksReady = false;

        if (isServer)
        {
            int blocksAmountInChunk = Width * Height;
            for (int i = 0; i < blocksAmountInChunk; i++)
                blockStates.Add(new BlockState(Material.Air));
        }

        //pre-generate chunk biomes
        Biome.GetBiomeAt(chunkPosition);

        if (isServer)
        {
            if (!hasPreviouslyBeenGenerated)
            {
                Debug.Log("Chunk [" + chunkPosition.chunkX + ", " + chunkPosition.dimension + "] is generating...");
                yield return StartCoroutine(GenerateBlocks());
            }
            
            //Load potentially already existing chunk data
            Debug.Log("Chunk [" + chunkPosition.chunkX + ", " + chunkPosition.dimension + "] is loading...");
            yield return StartCoroutine(LoadBlocks());
            
            LoadAllEntities();
            
            yield return StartCoroutine(BuildChunk());
            
            if(!hasPreviouslyBeenGenerated)
                GeneratingTickAllBlocks();
        }

        while (!areBlocksReady || blockStates.Count == 0)
            yield return new WaitForSeconds(0.1f);

        if (!isServer)
            yield return StartCoroutine(BuildChunk());

        if (isServer)
        {
            if(!hasPreviouslyBeenGenerated)
                GenerateAnimals();
            StartCoroutine(MobSpawnCycle());
            StartCoroutine(BlockRandomTickingCycle());
        }

        //Initialize all blocks after all blocks have been created
        yield return StartCoroutine(InitializeAllBlocks());

        GenerateBackgroundBlocks();
        GenerateSunlightSources();

        isLoaded = true;
        Debug.Log("Chunk [" + chunkPosition.chunkX + ", " + chunkPosition.dimension + "] is done...");

        if (isServer)
        {
            StartCoroutine(SelfDestructionChecker());
        }

        //Wait until neighboring chunks are loaded to initialize light
        while (true)
        {
            yield return new WaitForSeconds(0.2f);

            if (new ChunkPosition(chunkPosition.chunkX - 1, chunkPosition.dimension).IsChunkLoaded() &&
                new ChunkPosition(chunkPosition.chunkX + 1, chunkPosition.dimension).IsChunkLoaded())
            {
                LightManager.UpdateChunkLight(chunkPosition);
                isLightGenerated = true;
                break;
            }
        }
    }

    public void GenerateSunlightSources()
    {
        for (int x = 0; x < Width; x++)
            LightManager.UpdateSunlightInColumn(new LightColumn(chunkPosition.worldX + x, chunkPosition.dimension),
                false);
    }

    [Server]
    private IEnumerator LoadBlocks()
    {
        string path = WorldManager.world.GetPath() + "\\chunks\\" + chunkPosition.dimension + "\\" +
                      chunkPosition.chunkX + "\\blocks";

        if (File.Exists(path))
        {
            string[] lines = File.ReadAllLines(path);
            int i = 0;
            foreach (string line in lines)
            {
                try {
                    Location loc = new Location(int.Parse(line.Split('*')[0].Split(',')[0]),
                        int.Parse(line.Split('*')[0].Split(',')[1]),
                        chunkPosition.dimension);
                    Material mat = (Material)Enum.Parse(typeof(Material), line.Split('*')[1]);
                    BlockData data = new BlockData(line.Split('*')[2]);
                    BlockState state = new BlockState(mat, data);

                    loc.SetStateNoBlockChange(state);

                } catch (Exception e) { Debug.LogError("Error in chunk loading block, block save line: '" + line + "' error: " + e.Message + e.StackTrace); }

                //Every 100 blocks, wait 1 frame
                if (i % 50 == 0)
                    yield return 0;
                i++;
            }

        }
        areBlocksReady = true;
    }

    [Server]
    private IEnumerator GenerateBlocks()
    {
        //Assign world generator
        switch (WorldManager.world.template)
        {
            case WorldTemplate.Default:
                if (chunkPosition.dimension == Dimension.Overworld)
                    worldGenerator = new OverworldGenerator();
                else if (chunkPosition.dimension == Dimension.Nether)
                    worldGenerator = new NetherGenerator();
                break;
            case WorldTemplate.Skyblock:
                worldGenerator = new SkyblockGenerator();
                break;
        }
        
        chunkPosition.CreateChunkPath();

        //Generate Caves
        CaveGenerator.GenerateCavesForRegion(chunkPosition);

        //Generate Terrain Blocks
        Dictionary<Location, Material> terrainBlocks = null;
        Thread terrainThread = new Thread(() => { terrainBlocks = GenerateChunkTerrain(); });
        terrainThread.Start();

        while (terrainThread.IsAlive)
            yield return new WaitForSeconds(0.1f);

        foreach (KeyValuePair<Location, Material> terrainBlock in terrainBlocks)
        {
            BlockState state = new BlockState(terrainBlock.Value);

            terrainBlock.Key.SetStateNoBlockChange(state);
        }


        //Generate Structures
        Biome biome = GetBiome();
        for (int y = 1; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Location loc = new Location(chunkPosition.worldX + x, y, chunkPosition.dimension);
                BlockState state = worldGenerator.GenerateStructures(loc, biome);

                if (state.material != Material.Air)
                    loc.SetStateNoBlockChange(state);
            }

            yield return new WaitForSeconds(0f);
        }


        //Mark chunk as Generated
        string chunkDataPath = WorldManager.world.GetPath() + "\\chunks\\" + chunkPosition.dimension + "\\" +
                               chunkPosition.chunkX + "\\chunk";
        List<string> chunkDataLines = File.ReadAllLines(chunkDataPath).ToList();
        chunkDataLines.Add("hasBeenGenerated=true");
        File.WriteAllLines(chunkDataPath, chunkDataLines);
    }

    private IEnumerator BuildChunk()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = chunkPosition.worldX; x < chunkPosition.worldX + Width; x++)
                try
                {
                    Location loc = new Location(x, y, chunkPosition.dimension);
                    BlockState state = GetBlockState(loc);

                    LocalBlockChange(loc, state);
                }
                catch (Exception e)
                {
                    Debug.LogError("Error chunk build block: " + e.Message + e.StackTrace);
                }

            yield return new WaitForSeconds(0f);
        }

        donePlacingGeneratedBlocks = true;
    }

    [Server]
    public void SetBlockState(Location location, BlockState state)
    {
        if (!IsBlockLocal(location))
        {
            Debug.LogWarning("Tried setting block change in wrong chunk (" + location.x + ", " + location.y +
                             ") inside Chunk [" + chunkPosition.chunkX + ", " + chunkPosition.dimension + "]");
            return;
        }

        int2 chunkLocation = new int2(location.x - chunkPosition.worldX, location.y);
        int listIndex = chunkLocation.x + chunkLocation.y * Width;

        blockStates[listIndex] = state;
    }

    public BlockState GetBlockState(Location location)
    {
        if (!IsBlockLocal(location))
        {
            Debug.LogWarning("Tried getting block change in wrong chunk (" + location.x + ", " + location.y +
                             ") inside Chunk [" + chunkPosition.chunkX + ", " + chunkPosition.dimension + "]");
            return new BlockState(Material.Air);
        }

        if (blockStates.Count == 0)
            return new BlockState(Material.Air);

        int2 chunkLocation = new int2(location.x - chunkPosition.worldX, location.y);
        int listIndex = chunkLocation.x + chunkLocation.y * Width;

        return blockStates[listIndex];
    }

    [Server]
    public void DestroyChunk()
    {
        Debug.Log("Unloading chunk Chunk [" + chunkPosition.chunkX + ", " + chunkPosition.dimension + "]");
        UnloadEntities();
        NetworkServer.Destroy(gameObject);
    }

    [Server]
    public void UnloadEntities()
    {
        foreach (Entity entity in GetEntities())
            if (!(entity is Player))
                entity.Unload();
    }

    [Server]
    public static void CreateChunksAround(ChunkPosition loc, int distance)
    {
        for (int i = -distance; i < distance; i++)
        {
            ChunkPosition cPos = new ChunkPosition(loc.chunkX + i, loc.dimension);

            if (!cPos.IsChunkCreated())
                cPos.CreateChunk();
        }
    }

    [Server]
    private void GeneratingTickAllBlocks()
    {
        //Tick Blocks
        Block[] blockList = transform.GetComponentsInChildren<Block>();

        foreach (Block block in blockList)
        {
            if (block == null || block.transform == null)
                continue;

            try
            {
                block.GeneratingTick();
            }
            catch (Exception e)
            {
                Debug.LogError("Error in Block:GeneratingTick() caught: " + e.Message + e.StackTrace);
            }
        }
    }

    private IEnumerator InitializeAllBlocks()
    {
        //Initialize Blocks
        List<Block> blockList = new List<Block>(blocks.Values);
        int i = 0;
        startedInitializingAllBlocks = true;

        foreach (Block block in blockList)
        {
            if (block == null || block.transform == null)
                continue;

            try
            {
                block.Initialize();
                if (isServer)
                    block.ServerInitialize();
            }
            catch (Exception e)
            {
                Debug.LogError("Error in Block:Initialize() caught: " + e.Message + e.StackTrace);
            }

            i++;
            if (i % 20 == 0)
                yield return new WaitForSeconds(0);
        }
    }

    [Server]
    private IEnumerator SelfDestructionChecker()
    {
        float timePassedOutsideRenderDistance = 0f;
        while (true)
        {
            //Is outside one chunk of the render distance, begin self destruction
            if (chunkPosition.IsWithinDistanceOfPlayer(RenderDistance + 1))
            {
                timePassedOutsideRenderDistance = 0f;
            }
            else
            {
                timePassedOutsideRenderDistance += 1f;
                if (timePassedOutsideRenderDistance > OutsideRenderDistanceUnloadTime)
                {
                    timePassedOutsideRenderDistance = 0f;
                    DestroyChunk();
                }
            }

            yield return new WaitForSeconds(1f);
        }
    }
    
    [Server]
    public void DeleteNoLongerPresentEntitiesSaves()
    {
        List<String> presentUuids = new List<string>();
        foreach (Entity entity in GetEntities())
        {
            presentUuids.Add(entity.uuid);
        }
        
        string path = WorldManager.world.GetPath() + "\\chunks\\" + chunkPosition.dimension + "\\" +
                      chunkPosition.chunkX +
                      "\\entities";
        foreach (string entityPath in Directory.GetFiles(path))
        {
            string entityFile = entityPath.Split('\\')[entityPath.Split('\\').Length - 1];
            string entityUuid = entityFile.Split('.')[0];

            if(!presentUuids.Contains(entityUuid))
                File.Delete(entityPath);
        }
    }
    
    [Server]
    private void GenerateAnimals()
    {
        Random r = new Random(SeedGenerator.SeedByWorldLocation(new Location(chunkPosition.worldX, 0, chunkPosition.dimension)));
        
        if ((float) r.NextDouble() > animalGenerationChance)
            return;

        Biome biome = GetBiome();
        List<string> possibleAnimals = biome.biomeSpecificAnimals;
        if(biome.spawnDefaultOverworldAnimals)
            possibleAnimals.AddRange(DefaultOverworldAnimals);
        
        if(possibleAnimals.Count == 0)
            return;
        
        //Choose a random animal type for this chunk
        string entityType = possibleAnimals[r.Next(0, possibleAnimals.Count)];
        for (int amount = 0; amount < 4; amount++)
        {
            int x = r.Next(0, Width) + chunkPosition.worldX;
            Block topmostBlock = GetTopmostBlock(x, chunkPosition.dimension, true);

            //Return in case no block was found in column, may be the case in ex void worlds
            if (topmostBlock == null)
                continue;

            int y = topmostBlock.location.y + 1;

            Entity entity = Entity.Spawn(entityType);
            entity.Teleport(new Location(x, y, chunkPosition.dimension));
        }
    }
    
    [Server]
    private IEnumerator MobSpawnCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(20);
            
            if (GetMonsterCount() < monsterSpawnAmountCap && !chunkPosition.IsWithinDistanceOfPlayer(2))
            {
                TrySpawnMonster();
            }
        }
    }

    [Server]
    private void TrySpawnMonster()
    {
        //Define a random based on parameters specific to this chunk
        int randomSeed = SeedGenerator.SeedByParameters(
            WorldManager.world.seed, 
            chunkPosition.chunkX, 
            (int) chunkPosition.dimension, 
            (int)Time.time);
        Random r = new(randomSeed);
        
        //Decide which column in the chunk we should attempt to spawn the entity
        int worldXPosition = r.Next(0, Width) + chunkPosition.worldX;
        int worldHeight = Height;
        
        Dimension dimension = chunkPosition.dimension;
        //No need to iterate beyond nether bedrock roof
        if(dimension == Dimension.Nether)
            worldHeight = 128;
        
        List<Location> possibleSpawnLocations = new List<Location>();
        //Find a viable y position with a solid block beneath and air space above it,
        //As well having a correct light level for spawning
        int consecutiveEmptyBlocks = 0;
        for (int y = worldHeight - 1; y >= 0; y--)
        {
            Location loc = new(worldXPosition, y, dimension);
            Material mat = loc.GetMaterial();

            //Find solid block
            if (mat == Material.Air)
            {
                //Count how many air blocks we've passed
                consecutiveEmptyBlocks++;
                continue;
            }

            //Viable spawn location if, enough space, low enough light level
            if (consecutiveEmptyBlocks >= 2 && LightManager.GetLightValuesAt(loc).lightLevel <= monsterSpawningLightLevel)
                possibleSpawnLocations.Add(loc);
            
            consecutiveEmptyBlocks = 0;
        }

        //Return if no spawn locations where found
        if (possibleSpawnLocations.Count == 0)
            return;

        //Choose a random monster type based on the biome
        Biome biome = GetBiome();
        List<string> possibleEntityTypes = biome.biomeSpecificMonsters;
        if(biome.spawnDefaultOverworldMonsters)
            possibleEntityTypes.AddRange(DefaultOverworldMonsters);
        
        //TODO will fail if monster list is empty
        string entityType = possibleEntityTypes[r.Next(0, possibleEntityTypes.Count)];
        
        //Spawn a random monster at the location we decided
        Location spawnLocation = possibleSpawnLocations[r.Next(0, possibleSpawnLocations.Count)];
        Entity entity = Entity.Spawn(entityType);
        entity.Teleport(spawnLocation);
    }

    private IEnumerator BlockRandomTickingCycle()
    {
        float updateSpeed = 1f;
        int i = 0;
        while (true)
        {
            List<Block> blockList = new List<Block>(randomTickBlocks);
            foreach (Block block in blockList)
            {
                Random r = new Random(SeedGenerator.SeedByWorldLocation(block.location) + i);

                if (r.NextDouble() < updateSpeed / block.averageRandomTickDuration)
                    try
                    {
                        block.RandomTick();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Error in Block:RandomTick(): " + e.Message + ", Stack trace: '" + e.StackTrace + "', Following is probably stack trace to this debug message: ");
                    }
            }

            i++;
            yield return new WaitForSeconds(updateSpeed);
        }
    }

    private Dictionary<Location, Material> GenerateChunkTerrain()
    {
        Dictionary<Location, Material> blockList = new Dictionary<Location, Material>();

        for (int y = 0; y < Height; y++)
        for (int x = 0; x < Width; x++)
        {
            Location loc = new Location(x + chunkPosition.worldX, y, chunkPosition.dimension);
            Material mat = worldGenerator.GenerateTerrainBlock(loc);

            if (mat != Material.Air)
                blockList.Add(loc, mat);
        }

        return blockList;
    }

    private void GenerateBackgroundBlocks()
    {
        for (int x = 0; x < Width; x++)
            StartCoroutine(UpdateBackgroundBlockColumn(chunkPosition.worldX + x, false));

        donePlacingBackgroundBlocks = true;
    }

    private IEnumerator UpdateBackgroundBlockColumn(int x, bool updateLight)
    {
        yield return new WaitForSeconds(0);

        //Look up which blocks in the column will create background blocks
        List<int> transparentHeight = new List<int>();
        Dictionary<Location, Material> solidMaterials = new Dictionary<Location, Material>();
        for (int y = 0; y < Height; y++)
        {
            Location loc = new Location(x, y, chunkPosition.dimension);
            Material mat = loc.GetMaterial();
            
            //Clear out old background blocks
            if (backgroundBlocks.ContainsKey(new int2(loc.x, loc.y)))
            {
                Destroy(backgroundBlocks[new int2(loc.x, loc.y)].gameObject);
                backgroundBlocks.Remove(new int2(loc.x, loc.y));
            }
            
            if (BackgroundBlock.viableMaterials.ContainsKey(mat))
                solidMaterials.Add(loc, BackgroundBlock.viableMaterials[mat]);
            if (BackgroundBlock.transparentBackgrounds.Contains(mat))
                transparentHeight.Add(y);
        }

        for (int i = 0; i < solidMaterials.Count - 1; i++)
        {
            Location currentLoc = solidMaterials.Keys.ToArray()[i];
            Location nextLoc = solidMaterials.Keys.ToArray()[i + 1];
            int heightDistance = nextLoc.y - currentLoc.y;
            
            //Check so that blocks are not next to each other or too far apart
            if(heightDistance <= 1 || heightDistance > BackgroundBlock.MaxLengthFromSolid)
                continue;

            //Look up which material will be used for the background
            Material nextMat = solidMaterials[nextLoc];
            Material nextBackgroundMaterial = BackgroundBlock.viableMaterials[nextMat];

            //Create background blocks up to the next solid
            for (int y = currentLoc.y + 1; y < nextLoc.y; y++)
            {
                //Don't place background at transparent blocks (such as glass)
                if(transparentHeight.Contains(y))
                    continue;
                    
                GameObject backgroundblockObject = Instantiate(backgroundBlockPrefab, transform, true);
                BackgroundBlock backgroundBlock = backgroundblockObject.GetComponent<BackgroundBlock>();

                backgroundblockObject.transform.position = new Location(x, y, chunkPosition.dimension).GetPosition();
                backgroundBlock.material = nextBackgroundMaterial;
                backgroundBlocks.Add(new int2(x, y), backgroundBlock);

                if (updateLight)
                    ScheduleBlockLightUpdate(new Location(x, y, chunkPosition.dimension));
            }
        }
    }

    [Server]
    private void LoadAllEntities()
    {
        string path = WorldManager.world.GetPath() + "\\chunks\\" + chunkPosition.dimension + "\\" +
                      chunkPosition.chunkX +
                      "\\entities";

        if (!Directory.Exists(path))
            return;

        foreach (string entityPath in Directory.GetFiles(path))
        {
            string entityFile = entityPath.Split('\\')[entityPath.Split('\\').Length - 1];
            string entityType = entityFile.Split('.')[1];
            string entityUuid = entityFile.Split('.')[0];

            Entity.Spawn(entityType, entityUuid, transform.position + new Vector3(1, 0));
        }
    }

    private bool IsBlockLocal(Location loc)
    {
        return new ChunkPosition(loc).chunkX == chunkPosition.chunkX && loc.dimension == chunkPosition.dimension &&
               loc.y >= 0 && loc.y < Height;
    }

    [ClientRpc]
    public void BlockChange(Location loc, BlockState state)
    {
        if (!donePlacingGeneratedBlocks || isServer)
            return;

        StartCoroutine(ScheduleLocalBlockChange(loc, state));
    }

    private IEnumerator ScheduleLocalBlockChange(Location location, BlockState state)
    {
        //Schedule block change for next frame, since block data lists get synced late on clients
        yield return new WaitForSeconds(0f);
        LocalBlockChange(location, state);
    }

    public void LocalBlockChange(Location location, BlockState state)
    {
        int2 coordinates = new int2(location.x, location.y);
        Material mat = state.material;

        Type type = Type.GetType(mat.ToString());
        if (!type.IsSubclassOf(typeof(Block)))
            return;

        if (!IsBlockLocal(location))
        {
            Debug.LogWarning("Tried setting local block outside of chunk (" + location.x + ", " + location.y +
                             ") inside Chunk [" + chunkPosition.chunkX + ", " + chunkPosition.dimension +
                             "]");
            return;
        }

        //Before any blocks are removed or added, check wether current block is a sunlight source
        bool doesBlockChangeImpactSunlight = LightManager.DoesBlockInfluenceSunlight(location);

        //remove old block
        if (GetLocalBlock(location) != null)
        {
            if (isLoaded && GetLocalBlock(location).GetComponentInChildren<LightSource>() != null)
                LightManager.DestroySource(GetLocalBlock(location).GetComponentInChildren<LightSource>());

            Destroy(GetLocalBlock(location).gameObject);
            blocks.Remove(coordinates);
        }

        Block result = null;

        if (mat != Material.Air)
        {
            //Place new block
            GameObject blockObject = Instantiate(blockPrefab, transform, true);

            //Attach it to the object
            Block block = (Block) blockObject.AddComponent(type);

            blockObject.transform.position = location.GetPosition();

            //Add the block to block list
            if (blocks.ContainsKey(coordinates))
                blocks[coordinates] = block;
            else
                blocks.Add(coordinates, block);

            //Assign location to block
            block.location = location;
            
            //Dont initialize blocks when chunk is loading
            if (startedInitializingAllBlocks)
            {
                block.Initialize();
                if (isServer)
                    block.ServerInitialize();
            }

            result = blockObject.GetComponent<Block>();
        }

        if (mat == Material.Portal_Frame && netherPortal == null)
            netherPortal = (Portal_Frame) result;

        if (isLoaded)
        {
            if (doesBlockChangeImpactSunlight)
                LightManager.UpdateSunlightInColumn(new LightColumn(location.x, chunkPosition.dimension), true);
            StartCoroutine(UpdateBackgroundBlockColumn(location.x, true));
            ScheduleBlockLightUpdate(location);
        }
    }

    public void ScheduleBlockLightUpdate(Location loc)
    {
        StartCoroutine(scheduleBlockLightUpdate(loc));
    }
    

    private IEnumerator scheduleBlockLightUpdate(Location loc)
    {
        yield return new WaitForSeconds(0f);
        LightManager.UpdateBlockLight(loc);
    }

    [Server]
    public Location GeneratePortal(int x)
    {
        //Find an air pocket to place portal at
        int maxPortalHeight = chunkPosition.dimension == Dimension.Overworld ? Height : 128;
        for (int y = 0; y < maxPortalHeight; y++)
        {
            Location loc = new Location(x, y, chunkPosition.dimension);
            Location locAbove = new Location(x, y + 1, chunkPosition.dimension);
            if (loc.GetMaterial() != Material.Air && locAbove.GetMaterial() == Material.Air)
            {
                BlockState state = new BlockState(Material.Structure_Block, new BlockData("structure=Nether_Portal"));
                loc.SetState(state).Tick();
                return loc;
            }
        }

        //if no air pocket is found, place portal at y level 64
        Location defaultLocation = new Location(x, 64, chunkPosition.dimension);
        defaultLocation.SetMaterial(Material.Structure_Block).SetData(new BlockData("structure=Nether_Portal")).Tick();
        return defaultLocation;
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

    public Entity[] GetEntities()
    {
        Location chunkMinLoc = new Location(chunkPosition.worldX, 0, chunkPosition.dimension);
        Location chunkMaxLoc = new Location(chunkPosition.worldX + Width, Height, chunkPosition.dimension);
        Collider2D[] entityColliders = Physics2D.OverlapAreaAll(
            chunkMinLoc.GetPosition(), 
            chunkMaxLoc.GetPosition(), Entity.GetFilter().layerMask);
        List<Entity> entities = new List<Entity>();

        foreach (Collider2D entityCollider in entityColliders)
        {
            Entity entity = entityCollider.GetComponent<Entity>();

            if (entity != null)
                entities.Add(entity);
        }

        return entities.ToArray();
    }
    
    public int GetMonsterCount()
    {
        int amount = 0;
        
        foreach (Entity e in GetEntities())
            if (e is Monster)
                amount++;

        return amount;
    }

    public static Block GetTopmostBlock(int x, Dimension dimension, bool mustBeSolid)
    {
        Chunk chunk = new ChunkPosition(new Location(x, 0, dimension)).GetChunk();
        if (chunk == null)
            return null;

        return chunk.GetLocalTopmostBlock(x, mustBeSolid);
    }

    public Block GetLocalTopmostBlock(int x, bool mustBeSolid)
    {
        for (int y = Height - 1; y >= 0; y--)
        {
            Block block = GetLocalBlock(new Location(x, y, chunkPosition.dimension));

            if (block != null)
            {
                if (mustBeSolid && !block.solid)
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

    [ClientRpc]
    public void BlockBreakParticleEffect(Location loc, Color[] textureColors)
    {
        Random r = new Random();
        
        //First spawn a volume of particles and then modify properties of all particles
        foreach (Particle particle in Particle.ClientSpawnVolume(
                     loc.GetPosition() - new Vector2(0.5f, 0.5f), 
                     new Vector2(1, 1), new Vector2(1.3f, 4f), 
                     new float2(.3f, .7f), new int2(10 ,16), Color.white))
        {
            particle.color = textureColors[r.Next(textureColors.Length)];
            particle.doGravity = true;
        }
    }
}