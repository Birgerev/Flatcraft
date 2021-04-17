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

    private static readonly float mobSpawningChance = 0.005f;
    private static readonly List<string> MobSpawnTypes = new List<string> {"Chicken", "Sheep", "Cow", "Pig"};

    public GameObject blockPrefab;
    public GameObject backgroundBlockPrefab;

    public SyncList<BlockState> blockStates = new SyncList<BlockState>();
    public List<Block> randomTickBlocks = new List<Block>();
    public Dictionary<int2, Block> blocks = new Dictionary<int2, Block>();
    public Dictionary<int2, BackgroundBlock> backgroundBlocks = new Dictionary<int2, BackgroundBlock>();

    public WorldGenerator worldGenerator;

    [SyncVar]
    public ChunkPosition chunkPosition;
    
    [SyncVar]
    public bool areBlocksGenerated;
    public bool donePlacingGeneratedBlocks;
    public bool donePlacingBackgroundBlocks;
    public bool isLoaded;
    public bool isLightGenerated;
    public bool blocksInitialized;

    public Portal_Frame netherPortal;


    private void Start()
    {
        if (isServer)
        {
            WorldManager.instance.chunks.Add(chunkPosition, this);
        }
        
        StartCoroutine(CreateChunk());
    }

    IEnumerator CreateChunk()
    {
        WorldManager.instance.chunks[chunkPosition] = this;

        gameObject.name = "Chunk [" + chunkPosition.chunkX + " " + chunkPosition.dimension+ "]";
        transform.position = new Location(chunkPosition.worldX , 0, chunkPosition.dimension).GetPosition();

        isLoaded = false;
        donePlacingGeneratedBlocks = false;
        donePlacingBackgroundBlocks = false;
        isLightGenerated = false;
        blocksInitialized = false;
        
        if(isServer)
            areBlocksGenerated = false;
        
        if (isServer)
        {
            int blocksAmountInChunk = Width * Height;
            for (int i = 0; i < blocksAmountInChunk; i++)
            {
                blockStates.Add(new BlockState(Material.Air));
            }
        }
        
        //pre-generate chunk biomes
        Biome.GetBiomeAt(chunkPosition);
        
        if (isServer)
        {
            if (chunkPosition.HasBeenGenerated())
            {
                StartCoroutine(LoadBlocks());
                LoadAllEntities();
            }
            else
            {
                StartCoroutine(GenerateBlocks());
            }
        }

        while (!areBlocksGenerated || blockStates.Count == 0)
            yield return new WaitForSeconds(0.1f);

        if (isClient && !isServer)
        {
            StartCoroutine(BuildChunk());
        }
        
        while (!donePlacingGeneratedBlocks)
            yield return new WaitForSeconds(0.1f);

        if (isServer)
        {
            //Generate Tick all block (decay all necessary grass etc)
            StartCoroutine(MobSpawning());
            StartCoroutine(BlockRandomTicking());
        }
        
        //Initialize all blocks after all blocks have been created
        StartCoroutine(InitializeAllBlocks());

        while (!blocksInitialized)
            yield return new WaitForSeconds(0.1f);
        
        GenerateBackgroundBlocks();
        GenerateSunlightSources();
        
        isLoaded = true;
        
        if(isServer)
            StartCoroutine(SelfDestructionChecker());
        
        //Wait until neighboring chunks are loaded to initialize light
        while (true)
        {
            yield return new WaitForSeconds(0.2f);
            
            if (new ChunkPosition(chunkPosition.chunkX - 1, chunkPosition.dimension).IsChunkLoaded() && new ChunkPosition(chunkPosition.chunkX + 1, chunkPosition.dimension).IsChunkLoaded())
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
    IEnumerator LoadBlocks()
    {
        var path = WorldManager.world.getPath() + "\\chunks\\" + chunkPosition.dimension + "\\" +
                   chunkPosition.chunkX + "\\blocks";
        
        var lines = File.ReadAllLines(path);
        foreach (var line in lines)
        {
            Location loc = new Location(
                int.Parse(line.Split('*')[0].Split(',')[0]),
                int.Parse(line.Split('*')[0].Split(',')[1]),
                chunkPosition.dimension);
            Material mat = (Material) Enum.Parse(typeof(Material), line.Split('*')[1]);
            BlockData data = new BlockData(line.Split('*')[2]);
            BlockState state = new BlockState(mat, data);

            loc.SetStateNoBlockChange(state);
        }
        
        StartCoroutine(BuildChunk());

        while (!donePlacingGeneratedBlocks)
            yield return new WaitForSeconds(0.1f);
        
        areBlocksGenerated = true;
    }

    [Server]
    IEnumerator GenerateBlocks()
    {
        if(chunkPosition.dimension == Dimension.Overworld)
            worldGenerator = new OverworldGenerator();
        else if(chunkPosition.dimension == Dimension.Nether)
            worldGenerator = new NetherGenerator();

        chunkPosition.CreateChunkPath();
            
        //Generate Caves
        CaveGenerator.GenerateCavesForRegion(chunkPosition);
            
        //Generate Terrain Blocks
        Dictionary<Location, Material> terrainBlocks = null;
        var terrainThread = new Thread(() => { terrainBlocks = GenerateChunkTerrain(); });
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
        for (var y = 1; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                Location loc = new Location(chunkPosition.worldX + x, y, chunkPosition.dimension);
                BlockState state = worldGenerator.GenerateStructures(loc, biome);

                if (state.material != Material.Air)
                {
                    loc.SetStateNoBlockChange(state);
                }
            }
            yield return new WaitForSeconds(0f);
        }
        

        //Mark chunk as Generated
        var chunkDataPath = WorldManager.world.getPath() + "\\chunks\\" + chunkPosition.dimension + "\\" +
                            chunkPosition.chunkX + "\\chunk";
        var chunkDataLines = File.ReadAllLines(chunkDataPath).ToList();
        chunkDataLines.Add("hasBeenGenerated=true");
        File.WriteAllLines(chunkDataPath, chunkDataLines);
        
        StartCoroutine(BuildChunk());

        while (!donePlacingGeneratedBlocks)
            yield return new WaitForSeconds(0.1f);
        
        GeneratingTickAllBlocks();
        
        areBlocksGenerated = true;
    }

    IEnumerator BuildChunk()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = chunkPosition.worldX; x < chunkPosition.worldX + Width; x++)
            {
                Location loc = new Location(x, y, chunkPosition.dimension);
                BlockState state = GetBlockState(loc);
            
                LocalBlockChange(loc, state);
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
        int listIndex = chunkLocation.x + (chunkLocation.y * Width);
        
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
        int listIndex = chunkLocation.x + (chunkLocation.y * Width);

        return blockStates[listIndex];
    }

    [Server]
    public void DestroyChunk()
    {
        UnloadEntities();
        NetworkServer.Destroy(gameObject);
    }

    public void OnDestroy()
    {
        WorldManager.instance.chunks.Remove(chunkPosition);
    }

    [Server]
    public void UnloadEntities()
    {
        foreach(Entity entity in GetEntities())
        {
            if (!(entity is Player))
                entity.Unload();
        }
    }

    [Server]
    public static void CreateChunksAround(ChunkPosition loc, int distance)
    {
        for (var i = -distance; i < distance; i++)
        {
            var cPos = new ChunkPosition(loc.chunkX + i, loc.dimension);
            
            if (!cPos.IsChunkCreated())
                cPos.CreateChunk();
        }
    }

    [Server]
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
    
    IEnumerator InitializeAllBlocks()
    {
        //Initialize Blocks
        List<Block> blockList = new List<Block>(blocks.Values);
        int i = 0;
        
        foreach (var block in blockList)
        {
            if (block == null || block.transform == null)
                continue;

            block.Initialize();
            if(isServer)
                block.ServerInitialize();
            
            i++;
            if (i % 20 == 0)
            {
                yield return new WaitForSeconds(0);
            }
        }

        blocksInitialized = true;
    }

    [Server]
    private IEnumerator SelfDestructionChecker()
    {
        var timePassedOutsideRenderDistance = 0f;
        while (true)
        {
            if (chunkPosition.IsWithinDistanceOfPlayer(RenderDistance + 1))    //Is outside one chunk of the render distance, begin self destruction
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
    private IEnumerator MobSpawning()
    {
        while (true)
        {
            //Update neighbor chunks
            TrySpawnMobs();

            yield return new WaitForSeconds(1f / TickRate);
        }
    }
    
    private IEnumerator BlockRandomTicking()
    {
        float updateSpeed = 1f;
        int i = 0;
        while (true)
        {
            List<Block> blockList = new List<Block>(randomTickBlocks);
            foreach (Block block in blockList)
            {
                var r = new Random(block.location.GetHashCode() + i);
                
                if(r.NextDouble() < updateSpeed / block.averageRandomTickDuration) 
                    block.RandomTick();
            }

            i++;
            yield return new WaitForSeconds(updateSpeed);
        }
    }
    
    [Server]
    private void TrySpawnMobs()
    {
        var r = new Random();

        if (!(r.NextDouble() < mobSpawningChance / TickRate) || Entity.LivingEntityCount >= Entity.MaxLivingAmount) 
            return;

        var x = r.Next(0, Width) + chunkPosition.worldX;
        Block topmostBlock = GetTopmostBlock(x, chunkPosition.dimension, true);

        //Return in case no block was found in column, may be the case in ex void worlds
        if (topmostBlock == null)
            return;
            
        var y = topmostBlock.location.y + 1;
        var entities = MobSpawnTypes;
        entities.AddRange(GetBiome().biomeSpecificEntitySpawns);
        var entityType = entities[r.Next(0, entities.Count)];

        var entity = Entity.Spawn(entityType);
        entity.Teleport(new Location(x, y, chunkPosition.dimension));
    }

    private Dictionary<Location, Material> GenerateChunkTerrain()
    {
        var blockList = new Dictionary<Location, Material>();

        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
        {
            var loc = new Location(x + chunkPosition.worldX, y, chunkPosition.dimension);
            var mat = worldGenerator.GenerateTerrainBlock(loc);

            if (mat != Material.Air) blockList.Add(loc, mat);
        }

        return blockList;
    }
    
    private void GenerateBackgroundBlocks()
    {
        for (int x = 0; x < Width; x++)
        {
            StartCoroutine(UpdateBackgroundBlockColumn(chunkPosition.worldX + x, false));
        }

        donePlacingBackgroundBlocks = true;
    }

    IEnumerator UpdateBackgroundBlockColumn(int x, bool updateLight)
    {
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
            {
                lastViableMaterial = BackgroundBlock.viableMaterials[mat];
            }

            bool placeBackground = false;

            if (lastViableMaterial != Material.Air)
            {
                if (mat == Material.Air)
                    placeBackground = true;
                else if(loc.GetBlock() != null && !loc.GetBlock().solid)
                    placeBackground = true;
            }
            
            if(placeBackground)
            {
                GameObject blockObject = Instantiate(backgroundBlockPrefab, transform, true);
                BackgroundBlock backgroundBlock = blockObject.GetComponent<BackgroundBlock>();
                    
                blockObject.transform.position = loc.GetPosition();
                backgroundBlock.material = lastViableMaterial;
                backgroundBlocks.Add(new int2(loc.x, loc.y), backgroundBlock);

                if (updateLight)
                    StartCoroutine(scheduleBlockLightUpdate(loc));
            }
            
            if(!isLoaded && y % 10 == 0)
                yield return new WaitForSeconds(0);
        }
    }

    [Server]
    private void LoadAllEntities()
    {
        var path = WorldManager.world.getPath() + "\\chunks\\" + chunkPosition.dimension + "\\" + chunkPosition.chunkX +
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

    private bool IsBlockLocal(Location loc)
    {
        return (new ChunkPosition(loc).chunkX == chunkPosition.chunkX && loc.dimension == chunkPosition.dimension && loc.y >= 0 && loc.y < Height);
    }

    [ClientRpc]
    public void BlockChange(Location loc, BlockState state)
    {
        if (!donePlacingGeneratedBlocks || isServer)
            return;
        
        LocalBlockChange(loc, state);
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
            var block = (Block) blockObject.AddComponent(type);

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

        if(mat == Material.Portal_Frame)
        {
            netherPortal = (Portal_Frame)result;
        }

        if (isLoaded)
        {
            if (doesBlockChangeImpactSunlight)
                StartCoroutine(UpdateSunlightInColumn(location.x));
            StartCoroutine(UpdateBackgroundBlockColumn(location.x, true));
            StartCoroutine(scheduleBlockLightUpdate(location));
        }
    }

    //TODO try removing scheduling
    IEnumerator UpdateSunlightInColumn(int x)
    {
        yield return new WaitForSeconds(0f);
        LightManager.UpdateSunlightInColumn(new BlockColumn(x, chunkPosition.dimension), true);
    }


    IEnumerator scheduleBlockLightUpdate(Location loc)
    {
        yield return new WaitForSeconds(0f);
        LightManager.UpdateBlockLight(loc);
    }

    [Server]
    public Location GeneratePortal(int x)
    {
        //Find an air pocket to place portal at
        int maxPortalHeight = (chunkPosition.dimension == Dimension.Overworld) ? Chunk.Height : 128; 
        for (int y = 0; y < maxPortalHeight; y++)
        {
            Location loc = new Location(x, y, chunkPosition.dimension);
            if (loc.GetMaterial() == Material.Air)
            {
                (loc + new Location(0, -1)).SetMaterial(Material.Structure_Block).SetData(new BlockData("structure=Nether_Portal")).Tick();
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
        var entities = new List<Entity>();

        foreach (var e in Entity.entities)
            if (e.Location.x >= chunkPosition.worldX &&
                e.Location.x <= chunkPosition.worldX + Width &&
                e != null)
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
        for (var y = Height - 1; y >= 0; y--)
        {
            var block = GetLocalBlock(new Location(x, y, chunkPosition.dimension));

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