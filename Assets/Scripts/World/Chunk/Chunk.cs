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

    private static readonly float animalSpawnChance = 0.2f;
    private static readonly List<string> CommonAnimals = new List<string> {"Chicken", "Sheep", "Cow", "Pig"};

    public GameObject blockPrefab;
    public GameObject backgroundBlockPrefab;
    public List<Block> randomTickBlocks = new List<Block>();

    [SyncVar] public bool areBlocksGenerated;
    public bool donePlacingGeneratedBlocks;
    public bool donePlacingBackgroundBlocks;
    public bool isLoaded;
    public bool isLightGenerated;
    public bool blocksInitialized;

    public Portal_Frame netherPortal;
    public Dictionary<int2, BackgroundBlock> backgroundBlocks = new Dictionary<int2, BackgroundBlock>();
    public Dictionary<int2, Block> blocks = new Dictionary<int2, Block>();

    public SyncList<BlockState> blockStates = new SyncList<BlockState>();

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

        isLoaded = false;
        donePlacingGeneratedBlocks = false;
        donePlacingBackgroundBlocks = false;
        isLightGenerated = false;
        blocksInitialized = false;
        bool isGeneratingForFirstTime = false;

        if (isServer)
            areBlocksGenerated = false;

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
            if (chunkPosition.HasBeenGenerated())
            {
                Debug.Log("Chunk [" + chunkPosition.chunkX + ", " + chunkPosition.dimension + "] is loading...");
                StartCoroutine(LoadBlocks());
                LoadAllEntities();
            }
            else
            {
                isGeneratingForFirstTime = true;
                Debug.Log("Chunk [" + chunkPosition.chunkX + ", " + chunkPosition.dimension + "] is generating...");
                StartCoroutine(GenerateBlocks());
            }
        }

        while (!areBlocksGenerated || blockStates.Count == 0)
            yield return new WaitForSeconds(0.1f);

        if (!isServer)
            StartCoroutine(BuildChunk());

        while (!donePlacingGeneratedBlocks)
            yield return new WaitForSeconds(0.1f);

        if (isServer)
        {
            if(isGeneratingForFirstTime)
                GenerateAnimals();
            StartCoroutine(MobSpawnCycle());
            StartCoroutine(BlockRandomTickingCycle());
        }

        //Initialize all blocks after all blocks have been created
        StartCoroutine(InitializeAllBlocks());

        while (!blocksInitialized)
            yield return new WaitForSeconds(0.1f);

        
        GenerateBackgroundBlocks();
        GenerateSunlightSources();

        isLoaded = true;
        Debug.Log("Chunk [" + chunkPosition.chunkX + ", " + chunkPosition.dimension + "] is done...");

        if (isServer)
            StartCoroutine(SelfDestructionChecker());

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
            LightManager.UpdateSunlightInColumn(new BlockColumn(chunkPosition.worldX + x, chunkPosition.dimension),
                false);
    }

    [Server]
    private IEnumerator LoadBlocks()
    {
        string path = WorldManager.world.getPath() + "\\chunks\\" + chunkPosition.dimension + "\\" +
                      chunkPosition.chunkX + "\\blocks";

        string[] lines = File.ReadAllLines(path);
        foreach (string line in lines)
            try
            {
                Location loc = new Location(int.Parse(line.Split('*')[0].Split(',')[0]),
                    int.Parse(line.Split('*')[0].Split(',')[1]),
                    chunkPosition.dimension);
                Material mat = (Material) Enum.Parse(typeof(Material), line.Split('*')[1]);
                BlockData data = new BlockData(line.Split('*')[2]);
                BlockState state = new BlockState(mat, data);

                loc.SetStateNoBlockChange(state);
            }
            catch (Exception e)
            {
                Debug.LogError("Error in chunk loading block, block save line: '" + line + "' error: " + e.Message);
            }

        StartCoroutine(BuildChunk());

        while (!donePlacingGeneratedBlocks)
            yield return new WaitForSeconds(0.1f);

        areBlocksGenerated = true;
    }

    [Server]
    private IEnumerator GenerateBlocks()
    {
        if (chunkPosition.dimension == Dimension.Overworld)
            worldGenerator = new OverworldGenerator();
        else if (chunkPosition.dimension == Dimension.Nether)
            worldGenerator = new NetherGenerator();

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
        string chunkDataPath = WorldManager.world.getPath() + "\\chunks\\" + chunkPosition.dimension + "\\" +
                               chunkPosition.chunkX + "\\chunk";
        List<string> chunkDataLines = File.ReadAllLines(chunkDataPath).ToList();
        chunkDataLines.Add("hasBeenGenerated=true");
        File.WriteAllLines(chunkDataPath, chunkDataLines);

        StartCoroutine(BuildChunk());

        while (!donePlacingGeneratedBlocks)
            yield return new WaitForSeconds(0.1f);

        GeneratingTickAllBlocks();

        areBlocksGenerated = true;
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
                    Debug.LogError("Error chunk build block: " + e.Message);
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

            block.GeneratingTick();
        }
    }

    private IEnumerator InitializeAllBlocks()
    {
        //Initialize Blocks
        List<Block> blockList = new List<Block>(blocks.Values);
        int i = 0;

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
                Debug.LogError("Error in Block:Initialize(): " + e.Message);
            }

            i++;
            if (i % 20 == 0)
                yield return new WaitForSeconds(0);
        }

        blocksInitialized = true;
    }

    [Server]
    private IEnumerator SelfDestructionChecker()
    {
        float timePassedOutsideRenderDistance = 0f;
        while (true)
        {
            if (chunkPosition.IsWithinDistanceOfPlayer(RenderDistance + 1)
            ) //Is outside one chunk of the render distance, begin self destruction
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
    private void GenerateAnimals()
    {
        Random r = new Random(SeedGenerator.SeedByLocation(new Location(chunkPosition.worldX, 0, chunkPosition.dimension)));

        if ((float) r.NextDouble() > animalSpawnChance)
            return;

        List<string> entities = CommonAnimals;
        entities.AddRange(GetBiome().biomeSpecificAnimals);
        string entityType = entities[r.Next(0, entities.Count)];
        
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
            Random r = new Random();

            if (chunkPosition.IsWithinDistanceOfPlayer(3) &&
                r.NextDouble() < animalSpawnChance / TickRate)
            {
                
            }
                //TrySpawnMobs();

            yield return new WaitForSeconds(1f / TickRate);
        }
    }

    [Server]
    private void TrySpawnMobs()
    {
        Random r = new Random();

        if (!(r.NextDouble() < animalSpawnChance / TickRate))
            return;

        int x = r.Next(0, Width) + chunkPosition.worldX;
        Block topmostBlock = GetTopmostBlock(x, chunkPosition.dimension, true);

        //Return in case no block was found in column, may be the case in ex void worlds
        if (topmostBlock == null)
            return;

        int y = topmostBlock.location.y + 1;
        //List<string> entities = MobSpawnTypes;
        //entities.AddRange(GetBiome().biomeSpecificAnimals);
        //string entityType = entities[r.Next(0, entities.Count)];

        //Entity entity = Entity.Spawn(entityType);
        //entity.Teleport(new Location(x, y, chunkPosition.dimension));
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
                Random r = new Random(block.location.GetHashCode() + i);

                if (r.NextDouble() < updateSpeed / block.averageRandomTickDuration)
                    try
                    {
                        block.RandomTick();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Error in Block:RandomTick(): " + e.Message);
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

        Material lastViableMaterial = Material.Air;
        for (int y = Height - 1; y >= 0; y--)
        {
            Location loc = new Location(x, y, chunkPosition.dimension);
            Material mat = loc.GetMaterial();

            if (backgroundBlocks.ContainsKey(new int2(loc.x, loc.y)))
            {
                Destroy(backgroundBlocks[new int2(loc.x, loc.y)].gameObject);
                backgroundBlocks.Remove(new int2(loc.x, loc.y));
            }

            if (BackgroundBlock.viableMaterials.ContainsKey(mat))
                lastViableMaterial = BackgroundBlock.viableMaterials[mat];

            bool placeBackground = false;

            if (lastViableMaterial != Material.Air)
            {
                if (mat == Material.Air)
                    placeBackground = true;
                else if (loc.GetBlock() != null && !loc.GetBlock().solid)
                    placeBackground = true;
            }

            if (placeBackground)
            {
                GameObject blockObject = Instantiate(backgroundBlockPrefab, transform, true);
                BackgroundBlock backgroundBlock = blockObject.GetComponent<BackgroundBlock>();

                blockObject.transform.position = loc.GetPosition();
                backgroundBlock.material = lastViableMaterial;
                backgroundBlocks.Add(new int2(loc.x, loc.y), backgroundBlock);

                if (updateLight)
                    StartCoroutine(ScheduleBlockLightUpdate(loc));
            }

            if (!isLoaded && y % 10 == 0)
                yield return new WaitForSeconds(0);
        }
    }

    [Server]
    private void LoadAllEntities()
    {
        string path = WorldManager.world.getPath() + "\\chunks\\" + chunkPosition.dimension + "\\" +
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
            GameObject blockObject = null;

            blockObject = Instantiate(blockPrefab, transform, true);

            //Attach it to the object
            Block block = (Block) blockObject.AddComponent(type);

            blockObject.transform.position = location.GetPosition();

            //Add the block to block list
            if (blocks.ContainsKey(coordinates))
                blocks[coordinates] = block;
            else
                blocks.Add(coordinates, block);

            block.location = location;
            if (isLoaded)
            {
                block.Initialize();
                if (isServer)
                    block.ServerInitialize();
            }

            result = blockObject.GetComponent<Block>();
        }

        if (mat == Material.Portal_Frame)
            netherPortal = (Portal_Frame) result;

        if (isLoaded)
        {
            if (doesBlockChangeImpactSunlight)
                LightManager.UpdateSunlightInColumn(new BlockColumn(location.x, chunkPosition.dimension), true);
            StartCoroutine(UpdateBackgroundBlockColumn(location.x, true));
            StartCoroutine(ScheduleBlockLightUpdate(location));
        }
    }

    private IEnumerator ScheduleBlockLightUpdate(Location loc)
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
            if (loc.GetMaterial() == Material.Air)
            {
                (loc + new Location(0, -1)).SetMaterial(Material.Structure_Block)
                    .SetData(new BlockData("structure=Nether_Portal")).Tick();
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
        List<Entity> entities = new List<Entity>();

        foreach (Entity e in Entity.entities)
            if (e.Location.x >= chunkPosition.worldX &&
                e.Location.x <= chunkPosition.worldX + Width &&
                e != null)
                entities.Add(e);

        return entities.ToArray();
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
}