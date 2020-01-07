using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibNoise;
using System.IO;
using System.Threading;
using Unity.Burst;

[BurstCompile]
public class Chunk : MonoBehaviour
{
    public static float AutosaveDuration = 5;
    public static int Width = 16, Height = 255;
    public static int RenderDistance = 4;
    public static int SpawnChunkDistance = 0;
    public static int MinimumUnloadTime = 5;
    public static int TickRate = 1;

    public GameObject blockPrefab;

    public int ChunkPosition = 0;
    public bool isSpawnChunk = false;
    public bool isTickedChunk = false;
    public bool isLoaded = false;
    public bool isLoading = false;
    public int age = 0;

    public Chunk rightChunk;
    public Chunk leftChunk;
    public Dictionary<Vector2Int, Block> blocks = new Dictionary<Vector2Int, Block>();
    public Dictionary<int, Biome> cachedBiome = new Dictionary<int, Biome>();
    public Dictionary<Vector2Int, int> cachedRandomSeeds = new Dictionary<Vector2Int, int>();

    [Header("Cave Generation Settings")]
    public static float caveFrequency = 5;
    public static float caveLacunarity = 0.6f;
    public static float cavePercistance = 2;
    public static int caveOctaves = 4;
    public static float caveHollowValue = 2.2f;

    [Header("Ore Generation Settings")]
    public static int ore_coal_height = 128;
    public static double ore_coal_chance = 0.008f;

    public static int ore_iron_height = 64;
    public static double ore_iron_chance = 0.005f;

    public static int ore_gold_height = 32;
    public static double ore_gold_chance = 0.0015f;

    public static int ore_lapis_height = 32;
    public static double ore_lapis_chance = 0.0015f;

    public static int ore_redstone_height = 16;
    public static double ore_redstone_chance = 0.0015f;

    public static int ore_diamond_height = 16;
    public static double ore_diamond_chance = 0.0015f;

    public static int lava_height = 10;
    public static int sea_level = 62;


    public static float mobSpawningChance = 0.01f;
    public static List<string> mobSpawns = new List<string> { "Chicken", "Sheep" };



    public Dictionary<Vector2Int, string> blockChanges = new Dictionary<Vector2Int, string>();

    private void Start()
    {
        if (WorldManager.instance.chunks.ContainsKey(ChunkPosition))
        {
            Debug.LogWarning("A duplicate of Chunk ["+ChunkPosition+"] has been destroyed.");
            Destroy(gameObject);
            return;
        }

        isSpawnChunk = (ChunkPosition >= -SpawnChunkDistance && ChunkPosition <= SpawnChunkDistance);

        WorldManager.instance.chunks.Add(ChunkPosition, this);

        gameObject.name = "Chunk [" + ChunkPosition + "]";
        transform.position = new Vector3(ChunkPosition * Width, 0, 0);


        StartCoroutine(GenerateChunk());
        StartCoroutine(SaveLoop());
    }

    private void OnDestroy()
    {
        if (WorldManager.instance.chunks.ContainsKey(ChunkPosition) && (isLoaded || isLoading))
            WorldManager.instance.chunks.Remove(ChunkPosition);
    }

    private void Update()
    {
    }
    
    public static Chunk GetChunk(int cPos)
    {
        return GetChunk(cPos, true);
    }

    public static Chunk GetChunk(int cPos, bool loadIfNotFound)
    {
        Chunk chunk = null;

        WorldManager.instance.chunks.TryGetValue(cPos, out chunk);
        if (loadIfNotFound && chunk == null && cPos != 0)
        {
                chunk = LoadChunk(cPos);
        }

        return chunk;
    }

    public static int GetChunkPosFromWorldPosition(int worldPos)
    {
        int chunkPos = 0;
        if (worldPos >= 0)
        {
            chunkPos = (int)((float)worldPos / (float)Width);
        }
        else
        {
            chunkPos = Mathf.CeilToInt( ( (float)worldPos + 1f) / (float)Width) - 1;
        }

        return chunkPos;
    }

    public int GetMinXWorldPosition()
    {
        return Mathf.FloorToInt(this.ChunkPosition * (Width));
    }

    public static Chunk LoadChunk(int cPos)
    {
        //if (cPos != 0)// (GetChunk(0, false) == null || GetChunk(0, false).age < 4))
        //    return null;

        GameObject newChunk = Instantiate(WorldManager.instance.chunkPrefab);

        newChunk.GetComponent<Chunk>().ChunkPosition = cPos;

        return newChunk.GetComponent<Chunk>();
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
    
    IEnumerator Tick()
    {
        while (true)
        {
            if (inRenderDistance())
            {
                if (rightChunk == null && inRenderDistance(ChunkPosition+1))
                {
                    rightChunk = GetChunk(ChunkPosition + 1);
                }
                if (leftChunk == null && inRenderDistance(ChunkPosition - 1))
                {
                    leftChunk = GetChunk(ChunkPosition - 1);
                }
            }

            isTickedChunk = inRenderDistance(ChunkPosition, RenderDistance-1);
            if (!inRenderDistance())
            {
                UnloadChunk();
            }
            
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

        if(r.NextDouble() < mobSpawningChance / TickRate && Entity.livingEntityCount < Entity.MaxLivingAmount)
        {
            int x = r.Next(0, Width) + ChunkPosition*Width;
            int y = getTopmostBlock(x).position.y + 1;
            List<string> entities = mobSpawns;
            entities.AddRange(getBiome(x).biomeSpecificEntitySpawns);
            string entityType = entities[r.Next(0, entities.Count)];

            Entity entity = Entity.Spawn(entityType);
            entity.transform.position = new Vector3(x, y);
        }
    }

    public bool inRenderDistance()
    {
        return inRenderDistance(ChunkPosition);
    }

    public bool inRenderDistance(int cPos)
    {
        return inRenderDistance(cPos, RenderDistance);
    }

    public bool inRenderDistance(int cPos, int range)
    {
        if (cPos == 0)
            return true;

        cPos++;

        Vector2 playerPosition;


        if (Player.localInstance == null)
            playerPosition = Vector2.zero;
        else
            playerPosition = Player.localInstance.transform.position;

        int chunkXPosition = cPos * (int)Width;
        float distanceFromPlayer = Mathf.Abs((cPos * Width) - playerPosition.x);

        return distanceFromPlayer < range * Width;
    }

    public Dictionary<Vector2Int, Material> loadChunkTerrain()
    {
        cacheBiomes();
        cacheRandomSeeds();
        Dictionary<Vector2Int, Material> blocks = new Dictionary<Vector2Int, Material>();

        for (int y = 0; y <= Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Vector2Int pos = new Vector2Int(x + (ChunkPosition * Width), y);
                Material mat = getTheoreticalTerrainBlock(pos);
                
                if(mat != Material.Air)
                {
                    blocks.Add(pos, mat);
                }
            }
        }

        return blocks;
    }

    public void cacheBiomes()
    {
        for (int x = 0; x < Width; x++)
        {
            getBiome(x + (ChunkPosition * Width));
        }
    }

    public void cacheRandomSeeds()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y <= Height; y++)
            {
                seedByPosition(new Vector2Int(x + (ChunkPosition * Width), y));
            }
        }
    }

   IEnumerator GenerateChunk()
    {
        //Wait for scene to load
        yield return new WaitForSeconds(0.2f);

        patchNoise = new LibNoise.Generator.Perlin(0.6f, 0.8f, 0.8f, 2, WorldManager.world.seed, QualityMode.Low);
        lakeNoise = new LibNoise.Generator.Perlin(2, 0.8f, 5f, 2, WorldManager.world.seed, QualityMode.Low);
        caveNoise = new LibNoise.Generator.Perlin(caveFrequency, caveLacunarity, cavePercistance, caveOctaves, WorldManager.world.seed, QualityMode.High);

        isLoading = true;
        WorldManager.instance.amountOfChunksLoading++;


        Dictionary<Vector2Int, Material> terrainBlocks = null;
        Thread terrainThread = new Thread(() => { terrainBlocks = loadChunkTerrain(); });
        terrainThread.Start();

        while (terrainThread.IsAlive)
        {
            yield return new WaitForSeconds(0.5f);
        }
        
        int i = 0;
        foreach (KeyValuePair<Vector2Int, Material> entry in terrainBlocks)
        {
            setLocalBlock(entry.Key, entry.Value, "", false, false);
            i++;
            if(i % 10 == 1)
            {
                yield return new WaitForSeconds(0.05f);
            }


            float timePassed = 0f;
            while(!inRenderDistance())
            {
                yield return new WaitForSeconds(0.1f);
                timePassed += 0.1f;
                if (timePassed > 10f)
                {
                    Destroy(gameObject);
                    yield break;
                }
            }
        }

        for (int y = 0; y <= Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                GenerateStructures(new Vector2Int(x, y) + Vector2Int.CeilToInt(transform.position));
            }
            if(y < 80 && y % 4 == 0)
                yield return new WaitForSeconds(0.05f);
        }


        LoadAllEntities();

        StartCoroutine(Tick());
        StartCoroutine(TickAllBlocks());
        yield return new WaitForSecondsRealtime(1f);


        //Load block changes
        string path = WorldManager.world.getPath() + "\\region\\Overworld\\" + ChunkPosition + "\\blocks";
        if (File.Exists(path))
        {
            foreach (string line in File.ReadAllLines(path))
            {
                Vector2Int pos = new Vector2Int(int.Parse(line.Split('*')[0].Split(',')[0]), int.Parse(line.Split('*')[0].Split(',')[1]));
                Material mat = (Material)System.Enum.Parse(typeof(Material), line.Split('*')[1]);
                string data = line.Split('*')[2];
                
                setLocalBlock(pos, mat, data, false, false);
            }
        }

        isLoading = false;
        isLoaded = true;
        WorldManager.instance.amountOfChunksLoading--;
        
        StartCoroutine(GenerateLight());
        if (isSpawnChunk)
        {
            StartCoroutine(processLightLoop());
        }
    }

    IEnumerator GenerateLight()
    {
        //Fill sunlight source list
        int minChunkXPos = GetMinXWorldPosition();
        int maxChunkXPos = GetMinXWorldPosition() + Width - 1;

        for (int x = minChunkXPos; x <= maxChunkXPos; x++)
        {
            yield return new WaitForSecondsRealtime(0.05f);
            Block.UpdateSunlightSourceAt(x);
        }

        //Update Light Sources (not sunlight again)
        foreach (Block block in GetComponentsInChildren<Block>())
        {
            if (block.glowLevel > 0)
            {
                Block.UpdateLightAround(block.position);
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }
    }

    IEnumerator processLightLoop()
    {
        while (true)
        {
            if (Block.oldLight.Count > 0)
            {

                List<Vector2Int> oldLight = new List<Vector2Int>(Block.oldLight);
                Block.oldLight.Clear();
                List<KeyValuePair<Block, int>> lightToRender = null;

                Thread lightThread = new Thread(() => { lightToRender = processDirtyLight(oldLight); });
                lightThread.Start();

                while (lightThread.IsAlive)
                {
                    yield return new WaitForSeconds(0.1f);
                }

                //Render
                foreach (KeyValuePair<Block, int> entry in new List<KeyValuePair<Block, int>>(lightToRender))
                {
                    if (entry.Key == null)
                        continue;

                    entry.Key.RenderBlockLight(entry.Value);
                }
            }

            yield return new WaitForSecondsRealtime(0.05f);
        }
    }

    public List<KeyValuePair<Block, int>> processDirtyLight(List<Vector2Int> oldLight)
    {
        List<KeyValuePair<Block, int>> lightToRender = new List<KeyValuePair<Block, int>>();

        if (oldLight.Count == 0)
            return lightToRender;

        //Process
        foreach (Vector2Int pos in oldLight)
        {
            Block block = Chunk.getBlock(pos);
            if (block == null)
                continue;

            lightToRender.Add(new KeyValuePair<Block, int>(block, Block.GetLightLevel(pos)));
        }

        return lightToRender;
    }

    private void LoadAllEntities()
    {
        string path = WorldManager.world.getPath() + "\\region\\Overworld\\" + ChunkPosition + "\\entities";

        if (!Directory.Exists(path))
            return;

        foreach (string entityPath in Directory.GetFiles(path))
        {
            string entityFile = entityPath.Split('\\')[entityPath.Split('\\').Length - 1];
            string entityType = entityFile.Split('.')[1];
            int entityId = int.Parse(entityFile.Split('.')[0]);

            Entity entity = Entity.Spawn(entityType);
            entity.id = entityId;
            //Make sure the newly created entity is in the chunk, to make loading work correctly
            entity.transform.position = transform.position + new Vector3(1, 1);
        }
    }

    private void GenerateStructures(Vector2Int pos)
    {
        Block block = getBlock(pos);
        if (block == null)
            return;

        Material mat = block.GetMaterial();
        Biome biome = getBiome(pos.x);
        System.Random r = new System.Random(Chunk.seedByPosition(pos));

        if (biome.name == "forest")
        {
            if(mat == Material.Grass && getBlock((pos + new Vector2Int(0, 1))) == null)
            {
                //Trees
                if (r.Next(0, 100) <= 10)
                {
                    Chunk.setBlock(pos + new Vector2Int(0, 1), Material.Structure_Block, "structure=Tree|save=false", false, false);
                }

                //Large Trees
                if (r.Next(0, 100) <= 1)
                {
                    Chunk.setBlock(pos + new Vector2Int(0, 1), Material.Structure_Block, "structure=Large_Tree|save=false", false, false);
                }

                //Vegetation
                if (r.Next(0, 100) <= 25)
                {
                    Material[] vegetationMaterials = new Material[] { Material.Tall_Grass, Material.Red_Flower };

                    Chunk.setBlock(pos + new Vector2Int(0, 1), vegetationMaterials[r.Next(0, vegetationMaterials.Length)], "", false, false);
                }
            }
        }
        else if (biome.name == "desert")
        {
            if (mat == Material.Sand && getBlock((pos + new Vector2Int(0, 1))) == null)
            {
                //Cactie
                if (r.Next(0, 100) <= 8)
                {
                    Chunk.setBlock(pos + new Vector2Int(0, 1), Material.Structure_Block, "structure=Cactus|save=false", false, false);
                }
            }
        }

        //Generate Ores
        if (mat == Material.Stone)
        {
            if (r.NextDouble() < Chunk.ore_diamond_chance && pos.y <= Chunk.ore_diamond_height)
            {
                Chunk.setBlock(pos, Material.Structure_Block, "structure=Ore_Diamond|save=false", false, false);
            }
            else if (r.NextDouble() < Chunk.ore_redstone_chance && pos.y <= Chunk.ore_redstone_height)
            {
                Chunk.setBlock(pos, Material.Structure_Block, "structure=Ore_Redstone|save=false", false, false);
            }
            else if (r.NextDouble() < Chunk.ore_lapis_chance && pos.y <= Chunk.ore_lapis_height)
            {
                Chunk.setBlock(pos, Material.Structure_Block, "structure=Ore_Lapis|save=false", false, false);
            }
            else if (r.NextDouble() < Chunk.ore_gold_chance && pos.y <= Chunk.ore_gold_height)
            {
                Chunk.setBlock(pos, Material.Structure_Block, "structure=Ore_Gold|save=false", false, false);
            }
            else if (r.NextDouble() < Chunk.ore_iron_chance && pos.y <= Chunk.ore_iron_height)
            {
                Chunk.setBlock(pos, Material.Structure_Block, "structure=Ore_Iron|save=false", false, false);
            }
            else if (r.NextDouble() < Chunk.ore_coal_chance && pos.y <= Chunk.ore_coal_height)
            {
                Chunk.setBlock(pos, Material.Structure_Block, "structure=Ore_Coal|save=false", false, false);
            }
        }
    }

    IEnumerator SaveLoop()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(AutosaveDuration);

            StartCoroutine(AutosaveAllBlocks());
            

            //Save Block Changes
            Thread worldThread = new Thread(SaveBlockChanges);
            worldThread.Start();

            while (worldThread.IsAlive)
            {
                yield return new WaitForSeconds(0.1f);
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
            e.Save();
        }
    }

    public void SaveBlockChanges()
    {
        if (age <= 5)
            return;

        //Save Blocks
        if (blockChanges.Count > 0)
        {
            lock (blockChanges)
            {
                Dictionary<Vector2Int, string> changes = new Dictionary<Vector2Int, string>(blockChanges);
                blockChanges.Clear();

                string path = WorldManager.world.getPath() + "\\region\\Overworld\\" + ChunkPosition;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    Directory.CreateDirectory(path + "\\entities");
                    File.Create(path + "\\blocks").Close();
                }

                foreach (string line in File.ReadAllLines(path + "\\blocks"))
                {
                    Vector2Int linePos = new Vector2Int(int.Parse(line.Split('*')[0].Split(',')[0]), int.Parse(line.Split('*')[0].Split(',')[1]));
                    string lineData = line.Split('*')[1] + "*" + line.Split('*')[2];

                    if (!changes.ContainsKey(linePos))
                        changes.Add(linePos, lineData);
                }

                //Empty lines before writing
                File.WriteAllText(path + "\\blocks", "");

                TextWriter c = new StreamWriter(path + "\\blocks");

                foreach (KeyValuePair<Vector2Int, string> line in changes)
                {
                    c.WriteLine(line.Key.x + "," + line.Key.y + "*" + line.Value);
                }

                c.Close();
            }
        }
    }

    public void UnloadChunk()
    {
        if (isSpawnChunk)
            return;

        if (age >= MinimumUnloadTime*TickRate)
        {
            WorldManager.instance.chunks.Remove(ChunkPosition);
            Destroy(gameObject);
        }
    }

    public bool isBlockLocal(Vector2Int worldPos)
    {
        bool local = (GetChunkPosFromWorldPosition(worldPos.x) == ChunkPosition);
        
        if (worldPos.y < 0 || worldPos.y > Height)
            local = false;

        return local;
    }

    public static Block setBlock(Vector2Int worldPos, Material mat)
    {
        return setBlock(worldPos, mat, true);
    }

    public static Block setBlock(Vector2Int worldPos, Material mat, bool save)
    {
        return setBlock(worldPos, mat, "", save, true);
    }

    public static Block setBlock(Vector2Int worldPos, Material mat, string data, bool save, bool spreadTick)
    {
        Chunk chunk = GetChunk(GetChunkPosFromWorldPosition((int)worldPos.x), false);

        if (chunk == null)
            return null;

        return chunk.setLocalBlock(worldPos, mat, data, save, spreadTick);
    }

    public Block setLocalBlock(Vector2Int worldPos, Material mat)
    {
        return setLocalBlock(worldPos, mat, true);
    }

    public Block setLocalBlock(Vector2Int worldPos, Material mat, bool save)
    {
        return setLocalBlock(worldPos, mat, "", save, true);
    }

    public Block setLocalBlock(Vector2Int worldPos, Material mat, string data, bool save, bool spreadTick)
    {
        System.Type type = System.Type.GetType(mat.ToString());
        if (!type.IsSubclassOf(typeof(Block)))
            return null;

        if (!isBlockLocal(worldPos))
        {
            Debug.LogWarning("Tried setting local block outside of chunk (" + worldPos.x + ", " + worldPos.y + ") inside Chunk [" + ChunkPosition + "]");
            return null;
        }

        //remove old block
        if (getLocalBlock(worldPos) != null)
        {
            Destroy(getLocalBlock(worldPos).gameObject);
            blocks.Remove(worldPos);
        }
        
        if (save)
        {
            if (blockChanges.ContainsKey(worldPos))
                blockChanges.Remove(worldPos);
            blockChanges.Add(worldPos, mat.ToString() + "*" + data);
        }

        Block result = null;

        if (mat == Material.Air)
        {
            Block.SpreadTick(worldPos);
        }else
        {
            //Place new block
            GameObject block = null;

            block = Instantiate(blockPrefab);

            //Attach it to the object
            block.AddComponent(type);

            block.transform.parent = transform;
            block.transform.position = (Vector2)worldPos;

            //Add the block to block list
            if(!blocks.ContainsKey(worldPos))
                blocks.Add(worldPos, block.GetComponent<Block>());
            else
                blocks[worldPos] = block.GetComponent<Block>();

            block.GetComponent<Block>().data = Block.dataFromString(data);
            
            block.GetComponent<Block>().FirstTick();
            block.GetComponent<Block>().Tick(spreadTick);
            

            result = block.GetComponent<Block>();
        }

        if (isLoaded)
        {
            Block.UpdateSunlightSourceAt(worldPos.x);
            Block.UpdateLightAround(worldPos);
        }
        return result;
    }

    public static Block getBlock(Vector2Int worldPos)
    {
        Chunk chunk = GetChunk(GetChunkPosFromWorldPosition((int)worldPos.x), false);

        if (chunk == null)
            return null;

        Block block = chunk.getLocalBlock(worldPos);

        return block;
    }

    public Block getLocalBlock(Vector2Int worldPos)
    {
        if (!isBlockLocal(worldPos))
        {
            Debug.LogWarning("Tried getting local block outside of chunk (" + worldPos.x + ", " + worldPos.y + ") inside Chunk [" + ChunkPosition + "]");
            return null;
        }
        
        Block block = null;

        blocks.TryGetValue(worldPos, out block);

        return block;
    }
    
    LibNoise.Generator.Perlin caveNoise;
    LibNoise.Generator.Perlin patchNoise;
    LibNoise.Generator.Perlin lakeNoise;
    public Material getTheoreticalTerrainBlock(Vector2Int worldPos)
    {
        System.Random r = new System.Random(seedByPosition(worldPos));
        Material mat = Material.Air;

        List<Biome> biomes = getTwoMostProminantBiomes(worldPos.x);
        Biome biome = biomes[0];

        //-Terrain Generation-//
        float noiseValue = biome.blendNoiseValues(biomes[1], worldPos);

        //-Ground-//
        if (noiseValue > 0.1f)
        {
            if (biome.name == "desert" || biome.name == "lake")
            {
                mat = Material.Sand;
            }
            else if (biome.name == "forest")
            {
                mat = Material.Grass;
            }

            if (noiseValue > 0.5f)
            {
                mat = Material.Stone;
            }
        }

        //-Lakes-//
        if (mat == Material.Air && worldPos.y <= sea_level && biome.name == "lake")
        {
            mat = Material.Water;
        }

        //-Dirt & Gravel Patches-//
        if (mat == Material.Stone)
        {
            if (Mathf.Abs((float)caveNoise.GetValue((float)worldPos.x / 20, (float)worldPos.y / 20)) > 7.5f)
            {
                mat = Material.Dirt;
            }
            if (Mathf.Abs((float)caveNoise.GetValue((float)worldPos.x / 20 + 100, (float)worldPos.y / 20, 200)) > 7.5f)
            {
                mat = Material.Gravel;
            }
        }

        //-Sea-//
        if (mat == Material.Air && worldPos.y <= sea_level)
        {
            mat = Material.Water;
        }

        //-Caves-//
        if(noiseValue > 0.1f)
        {
            double caveValue =
                (caveNoise.GetValue((float)worldPos.x / 20, (float)worldPos.y / 20) + 4.0f) / 4f;
            if (caveValue > caveHollowValue)
            {
                mat = Material.Air;

        //-Lava Lakes-//
                if (worldPos.y <= lava_height)
                    mat = Material.Lava;
            }
        }

        //-Bedrock Generation-//
        if (worldPos.y <= 4)
        {
            //Fill layer 0 and then progressively less chance of bedrock further up
            if (worldPos.y == 0)
                mat = Material.Bedrock;
            else if (r.Next(0, (int)worldPos.y+2) <= 1)
                mat = Material.Bedrock;
        }

        return mat;
    }

    public Entity[] GetEntities()
    {
        List<Entity> entities = new List<Entity>();

        int chunkXPosition = this.GetMinXWorldPosition();
        foreach (Entity e in entities)
        {
            if (e.transform.position.x >= chunkXPosition && e.transform.position.x <= (chunkXPosition + Width))
                entities.Add(e);
        }

        return entities.ToArray();
    }

    public static Block getTopmostBlock(int x)
    {
        Chunk chunk = GetChunk(GetChunkPosFromWorldPosition(x), false);
        if (chunk == null)
            return null;

        return chunk.getLocalTopmostBlock(x);
    }

    public Block getLocalTopmostBlock(int x)
    {
        for(int y = Height; y > 0; y--)
        {
            if(getLocalBlock(new Vector2Int(x, y)) != null)
            {
                return getLocalBlock(new Vector2Int(x, y));
            }
        }
        return null;
    }

    public static int seedByPosition(Vector2Int pos)
    {
        Chunk chunk = Chunk.GetChunk(Chunk.GetChunkPosFromWorldPosition(pos.x), false);
        int seed = 0;

        if (chunk == null)
            return seed;

        chunk.cachedRandomSeeds.TryGetValue(pos, out seed);
        if (seed == 0) {
            seed = new System.Random((WorldManager.world.seed.ToString() + ", " + pos.x + ", " + pos.y).GetHashCode()).Next(int.MinValue, int.MaxValue);
            chunk.cachedRandomSeeds[pos] = seed;
        }

        return seed;
    }

    public static Biome getBiome(int pos)
    {
        Chunk chunk = Chunk.GetChunk(Chunk.GetChunkPosFromWorldPosition(pos), false);
        Biome biome = null;

        if (chunk == null)
            return null;

        if (chunk.cachedBiome.ContainsKey(pos))
        {
            biome = chunk.cachedBiome[pos];
        }
        else
        {
            biome = getTwoMostProminantBiomes(pos)[0];
            chunk.cachedBiome.Add(pos, biome);
        }

        return biome;
    }

    public static List<Biome> getTwoMostProminantBiomes(int pos)
    {
        List<Biome> biomes = new List<Biome>(WorldManager.instance.biomes);

        biomes.Sort((x, y) => x.getBiomeValueAt(pos).CompareTo(y.getBiomeValueAt(pos)));

        return biomes;
    }
}