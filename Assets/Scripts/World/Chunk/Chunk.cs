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
    public const int RenderDistance = 6;
    public const int AmountOfChunksInRegion = 16;
    public const int OutsideRenderDistanceUnloadTime = 10;
    public const int TickRate = 1;

    private static readonly float mobSpawningChance = 0.005f;
    private static readonly List<string> MobSpawnTypes = new List<string> {"Chicken", "Sheep", "Cow", "Pig"};
    public int age;

    public GameObject blockPrefab;
    public GameObject backgroundBlockPrefab;

    private SyncList<BlockState> blockStates = new SyncList<BlockState>();
    public Dictionary<int2, Block> blocks = new Dictionary<int2, Block>();
    public Dictionary<int2, BackgroundBlock> backgroundBlocks = new Dictionary<int2, BackgroundBlock>();

    public WorldGenerator worldGenerator;

    [SyncVar]
    public ChunkPosition chunkPosition;
    
    [SyncVar]
    public bool isGenerated;
    public bool donePlacingGeneratedBlocks;
    public bool isLoaded;
    

    public Portal_Frame netherPortal;

    private void Start()
    {
        WorldManager.instance.chunks.Add(chunkPosition, this);

        if(isServer)
            StartCoroutine(SelfDestructionChecker());

        gameObject.name = "Chunk [" + chunkPosition.chunkX + " " + chunkPosition.dimension+ "]";
        transform.position = new Vector3(chunkPosition.worldX, 0, 0);

        StartCoroutine(CreateChunk());
    }

    IEnumerator CreateChunk()
    {
        WorldManager.instance.amountOfChunksLoading++;
        isLoaded = false;
        donePlacingGeneratedBlocks = false;
        if(isServer)
            isGenerated = false;
        

        if (isServer)
        {
            int blocksAmountInChunk = Width * Height;
            BlockState emptyBlockState = new BlockState(Material.Air);
            blockStates.AddRange(Enumerable.Repeat(emptyBlockState, blocksAmountInChunk));
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

        while (!isGenerated || blockStates.Count == 0)
            yield return new WaitForSeconds(0.1f);
        
        yield return new WaitForSeconds(1f);

        if (isClient)
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

        yield return new WaitForSeconds(1f);
        
        //Initialize all blocks after all blocks have been created
        InitializeAllBlocks();
        StartCoroutine(GenerateBackgroundBlocks());
        GenerateSunlight();
        
        isLoaded = true;
        WorldManager.instance.amountOfChunksLoading--;
        
        //Wait until neighboring chunks are loaded to initialize light
        while (true)
        {
            yield return new WaitForSeconds(1f);
            if (new ChunkPosition(chunkPosition.chunkX - 1, chunkPosition.dimension).IsChunkLoaded() && new ChunkPosition(chunkPosition.chunkX + 1, chunkPosition.dimension).IsChunkLoaded())
            {
                LightManager.UpdateChunkLight(chunkPosition);
                break;
            }
        }
    }

    [Server]
    IEnumerator LoadBlocks()
    {
        var path = WorldManager.world.getPath() + "\\region\\" + chunkPosition.dimension + "\\" +
                   chunkPosition.chunkX + "\\blocks";
        
        var lines = File.ReadAllLines(path);
        var i = 0;
        foreach (var line in lines)
        {
            Location loc = new Location(
                int.Parse(line.Split('*')[0].Split(',')[0]),
                int.Parse(line.Split('*')[0].Split(',')[1]),
                chunkPosition.dimension);
            Material mat = (Material) Enum.Parse(typeof(Material), line.Split('*')[1]);
            BlockData data = new BlockData(line.Split('*')[2]);
            BlockState state = new BlockState(mat, data);

            loc.SetState(state);
                
            i++;
            if (i % 10 == 1) 
                yield return new WaitForSeconds(0.05f);
        }

        isGenerated = true;
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

            if (y < 80 && y % 4 == 0)
                yield return new WaitForSeconds(0.05f);
        }
        

        //Mark chunk as Generated
        var chunkDataPath = WorldManager.world.getPath() + "\\region\\" + chunkPosition.dimension + "\\" +
                            chunkPosition.chunkX + "\\chunk";
        var chunkDataLines = File.ReadAllLines(chunkDataPath).ToList();
        chunkDataLines.Add("hasBeenGenerated=true");
        File.WriteAllLines(chunkDataPath, chunkDataLines);
        
        StartCoroutine(BuildChunk());

        while (!donePlacingGeneratedBlocks)
            yield return new WaitForSeconds(0.5f);
        
        GeneratingTickAllBlocks();
        
        isGenerated = true;
    }

    IEnumerator BuildChunk()
    {
        for (int x = chunkPosition.worldX; x < chunkPosition.worldX + Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Location loc = new Location(x, y, chunkPosition.dimension);
                BlockState state = GetBlockState(loc);
            
                LocalBlockChange(loc, state);
                
                //if (y % 10 == 1) yield return new WaitForSeconds(0.05f);
            }
        }
        
        yield return new WaitForSeconds(0.05f);
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
        
        int2 chunkLocation = new int2(location.x - chunkPosition.worldX, location.y);
        int listIndex = chunkLocation.x + (chunkLocation.y * Width);
        
        return blockStates[listIndex];
    }

    [Server]
    public void DestroyChunk()
    {
        UnloadEntities();

        WorldManager.instance.chunks.Remove(chunkPosition);
        if (!isLoaded)
            WorldManager.instance.amountOfChunksLoading--;

        NetworkServer.Destroy(gameObject);
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
    
    private void InitializeAllBlocks()
    {
        //Initialize Blocks
        var blockList = transform.GetComponentsInChildren<Block>();

        foreach (var block in blockList)
        {
            if (block == null || block.transform == null)
                continue;

            block.Initialize();
        }
    }

    [Server]
    private IEnumerator SelfDestructionChecker()
    {
        var timePassedOutsideRenderDistance = 0f;
        while (true)
        {
            if (Player.localEntity != null && Player.localEntity.Location.dimension != chunkPosition.dimension)        //If chunk is not in the same dimension as the player, self destruct
            {
                DestroyChunk();
            }
            
            if (!chunkPosition.IsWithinDistanceOfPlayer(RenderDistance + 1))    //Is outside one chunk of the render distance, begin self destruction
            {
                timePassedOutsideRenderDistance += 1f;
                if (timePassedOutsideRenderDistance > OutsideRenderDistanceUnloadTime)
                {
                    timePassedOutsideRenderDistance = 0f;
                    DestroyChunk();
                }
            }
            else
            {
                timePassedOutsideRenderDistance = 0f;
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

            age++;
            yield return new WaitForSeconds(1f / TickRate);
        }
    }
    
    private IEnumerator BlockRandomTicking()
    {
        int i = 0;
        while (true)
        {
            foreach (Block block in blocks.Values)
            {
                if (block.averageRandomTickDuration > 0)
                {
                    var r = new Random(block.location.GetHashCode() + i);
                    
                    if(r.NextDouble() > 1 / block.averageRandomTickDuration) 
                        block.RandomTick();
                }
            }

            i++;
            yield return new WaitForSeconds(1);
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
    
    IEnumerator GenerateBackgroundBlocks()
    {
        for (int x = 0; x < Width; x++)
        {
            UpdateBackgroundBlockColumn(chunkPosition.worldX + x, false);
            yield return new WaitForSeconds(0.1f);
        }
    }

    //TODO two blocks have to be build in column before background blocks appear on client,
    //probably since blockchange might be triggered before blockstate list gets updated
    public void UpdateBackgroundBlockColumn(int x, bool updateLight)
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

            if (BackgroundBlock.viableMaterials.Contains(mat))
            {
                lastViableMaterial = mat;
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
        }
    }

    private void GenerateSunlight()
    {
        var minXPos = chunkPosition.worldX;
        var maxXPos = chunkPosition.worldX + Width - 1;

        for (var x = minXPos; x <= maxXPos; x++) 
            LightManager.UpdateSunlightInColumn(x);
    }

    [Server]
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

    private bool IsBlockLocal(Location loc)
    {
        return (new ChunkPosition(loc).chunkX == chunkPosition.chunkX && loc.dimension == chunkPosition.dimension && loc.y >= 0 && loc.y < Height);
    }

    [ClientRpc]
    public void BlockChange(Location loc, BlockState state)
    {
        if (!donePlacingGeneratedBlocks)
            return;
        
        LocalBlockChange(loc, state);
    }
    
    public void LocalBlockChange(Location loc, BlockState state)
    {
        int2 pos = new int2(loc.GetPosition());
        Material mat = state.material;
        BlockData data = state.data;
        
        var type = Type.GetType(mat.ToString());
        if (!type.IsSubclassOf(typeof(Block)))
            return;

        if (!IsBlockLocal(loc))
        {
            Debug.LogWarning("Tried setting local block outside of chunk (" + loc.x + ", " + loc.y +
                             ") inside Chunk [" + chunkPosition.chunkX + ", " + chunkPosition.dimension +
                             "]");
            return;
        }

        //Before any blocks are removed or added, check wether current block is a sunlight source
        bool doesBlockChangeImpactSunlight = LightManager.DoesBlockInfluenceSunlight(new int2(loc.GetPosition()));

        //remove old block
        if (GetLocalBlock(loc) != null)
        {
            if (isLoaded && GetLocalBlock(loc).GetComponentInChildren<LightSource>() != null)
                LightManager.DestroySource(GetLocalBlock(loc).GetComponentInChildren<LightSource>().gameObject);

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

            block.location = loc;
            if (isLoaded)
                block.ScheduleBlockInitialization();

            result = blockObject.GetComponent<Block>();
        }

        if(mat == Material.Portal_Frame)
        {
            netherPortal = (Portal_Frame)result;
        }

        if (isLoaded)
        {
            if (doesBlockChangeImpactSunlight)
                LightManager.UpdateSunlightInColumn(loc.x);

            UpdateBackgroundBlockColumn(loc.x, true);
            StartCoroutine(scheduleBlockLightUpdate(loc));
        }

        return;
    }

    IEnumerator scheduleBlockLightUpdate(Location loc)
    {
        yield return new WaitForSeconds(0.01f);
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