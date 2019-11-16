using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibNoise;
using System.IO;

public class Chunk : MonoBehaviour
{
    public static float AutosaveDuration = 15;
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
    public static float mobSpawningAmountCap = 2;
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

        /*
        for (int i = GetMinXWorldPosition() -1; i < GetMinXWorldPosition() + 17; i++)
        {
            print("isBlockLocal("+i+", 5): "+isBlockLocal(new Vector2Int(i, 5)));
        }*/

        /*for (int i = GetMinXWorldPosition() - 1; i < GetMinXWorldPosition() + 17; i++)
        {
            print(ChunkPosition + " GetChunkPosFromWorldPosition(" + i + "): " + GetChunkPosFromWorldPosition((int)i));
        }*/

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

        if (WorldManager.instance.chunks.ContainsKey(cPos))
            chunk = WorldManager.instance.chunks[cPos];
        if (chunk == null)
        {
            if (cPos != 0 && loadIfNotFound)
                chunk = LoadChunk(cPos);
        }

        return chunk;
    }

    public static int GetChunkPosFromWorldPosition(int worldPos)
    {
        int chunkPos = 0;
        if (worldPos >= 0)
        {
            chunkPos = Mathf.FloorToInt((float)worldPos / (float)Width);
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
        if (cPos != 0 && (GetChunk(0, false) == null || GetChunk(0, false).age < 4))
            return null;

        GameObject newChunk = Instantiate(WorldManager.instance.chunkPrefab);

        newChunk.GetComponent<Chunk>().ChunkPosition = cPos;

        return newChunk.GetComponent<Chunk>();
    }

    IEnumerator TickAllBlocks()
    {
        //Tick Blocks
        Block[] blocks = transform.GetComponentsInChildren<Block>();

        int tickedBlocks = 0;
        foreach (Block block in blocks)
        {
            if (block == null || block.transform == null)
                continue;

            if (age == 0)
                block.GeneratingTick();

            tickedBlocks++;
            block.Tick(false);
        }
        yield return new WaitForSecondsRealtime(0f);
    }

    IEnumerator AutosaveAllBlocks()
    {
        //Tick Blocks
        Block[] blocks = transform.GetComponentsInChildren<Block>();

        float timePerBlock = 5 / blocks.Length;
        foreach (Block block in blocks)
        {
            if (block == null || block.transform == null)
                continue;

            if (block.autosave == false)
                continue;

            block.Tick(false);
            block.Autosave();
        }
        yield return new WaitForSecondsRealtime(0);

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

        if(r.NextDouble() < mobSpawningChance / TickRate &&GetEntities().Length < mobSpawningAmountCap)
        {
            int x = r.Next(0, Width) + ChunkPosition*Width;
            int y = getTopmostBlock(x).getPosition().y + 1;
            List<string> entities = mobSpawns;
            entities.AddRange(getMostProminantBiome(x).biomeSpecificEntitySpawns);
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

    public void RegenerateChunk()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        StartCoroutine(GenerateChunk());
    }

    IEnumerator GenerateChunk()
    {
        patchNoise = new LibNoise.Generator.Perlin(0.6f, 0.8f, 0.8f, 2, WorldManager.world.seed, QualityMode.Low);
        lakeNoise = new LibNoise.Generator.Perlin(2, 0.8f, 5f, 2, WorldManager.world.seed, QualityMode.Low);
        caveNoise = new LibNoise.Generator.Perlin(caveFrequency, caveLacunarity, cavePercistance, caveOctaves, WorldManager.world.seed, QualityMode.High);

        isLoading = true;

        WorldManager.instance.amountOfChunksLoading++;
        for (int y = 0; y <= Height; y++)
        {
            int blocksGenerated = 0;
            for (int x = 0; x < Width; x++)
            {
                Block loadedBlock = loadTheoreticalBlock(new Vector2Int(x, y) + Vector2Int.CeilToInt(transform.position));

                if (loadedBlock != null)
                    blocksGenerated++;
            }
            if (blocksGenerated > 0 && y % 1 == 5)
            {
                yield return new WaitForSecondsRealtime(0.05f);
            }

            float timeNotInRenderdistance = 0;
            while (!inRenderDistance() && timeNotInRenderdistance < 10) { 

                yield return new WaitForSecondsRealtime(0.1f);
                timeNotInRenderdistance += 0.1f;
                WorldManager.instance.amountOfChunksLoading--;
            }
            if(timeNotInRenderdistance >= 10)
            {
                Destroy(gameObject);
                yield break;
            }
        }

        for (int y = 0; y <= Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                GenerateStructures(new Vector2Int(x, y) + Vector2Int.CeilToInt(transform.position));
            }
        }


        LoadAllEntities();

        StartCoroutine(Tick());
        yield return new WaitForSecondsRealtime(1f);
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
                
                setLocalBlock(pos, mat, data, false);
            }
        }

        isLoading = false;
        isLoaded = true;
        WorldManager.instance.amountOfChunksLoading--;
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
        Biome biome = getMostProminantBiome(pos.x);
        System.Random r = new System.Random(Chunk.seedByPosition(pos));

        if (biome.name == "forest")
        {
            if(mat == Material.Grass && getBlock((pos + new Vector2Int(0, 1))) == null)
            {
                //Trees
                if (r.Next(0, 100) <= 10)
                {
                    Chunk.setBlock(pos + new Vector2Int(0, 1), Material.Structure_Block, "structure=Tree|save=false", false);
                }

                //Large Trees
                if (r.Next(0, 100) <= 1)
                {
                    Chunk.setBlock(pos + new Vector2Int(0, 1), Material.Structure_Block, "structure=Large_Tree|save=false", false);
                }

                //Vegetation
                if (r.Next(0, 100) <= 25)
                {
                    Material[] vegetationMaterials = new Material[] { Material.Tall_Grass, Material.Red_Flower };

                    Chunk.setBlock(pos + new Vector2Int(0, 1), vegetationMaterials[r.Next(0, vegetationMaterials.Length)], false);
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
                    Chunk.setBlock(pos + new Vector2Int(0, 1), Material.Structure_Block, "structure=Cactus|save=false", false);
                }
            }
        }

        //Generate Ores
        if (mat == Material.Stone)
        {
            if (r.NextDouble() < Chunk.ore_diamond_chance && pos.y <= Chunk.ore_diamond_height)
            {
                Chunk.setBlock(pos, Material.Structure_Block, "structure=Ore_Diamond|save=false", false);
            }
            else if (r.NextDouble() < Chunk.ore_redstone_chance && pos.y <= Chunk.ore_redstone_height)
            {
                Chunk.setBlock(pos, Material.Structure_Block, "structure=Ore_Redstone|save=false", false);
            }
            else if (r.NextDouble() < Chunk.ore_lapis_chance && pos.y <= Chunk.ore_lapis_height)
            {
                Chunk.setBlock(pos, Material.Structure_Block, "structure=Ore_Lapis|save=false", false);
            }
            else if (r.NextDouble() < Chunk.ore_gold_chance && pos.y <= Chunk.ore_gold_height)
            {
                Chunk.setBlock(pos, Material.Structure_Block, "structure=Ore_Gold|save=false", false);
            }
            else if (r.NextDouble() < Chunk.ore_iron_chance && pos.y <= Chunk.ore_iron_height)
            {
                Chunk.setBlock(pos, Material.Structure_Block, "structure=Ore_Iron|save=false", false);
            }
            else if (r.NextDouble() < Chunk.ore_coal_chance && pos.y <= Chunk.ore_coal_height)
            {
                Chunk.setBlock(pos, Material.Structure_Block, "structure=Ore_Coal|save=false", false);
            }
        }
    }

    IEnumerator SaveLoop()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(AutosaveDuration);
            StartCoroutine(AutosaveAllBlocks());
            Save();
        }
    }

    public void Save()
    {
        //Save Blocks
        if (blockChanges.Count > 0)
        {
            string path = WorldManager.world.getPath() + "\\region\\Overworld\\" + ChunkPosition;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Directory.CreateDirectory(path+"\\entities");
                File.Create(path + "\\blocks").Close();
            }

            foreach (string line in File.ReadAllLines(path + "\\blocks"))
            {
                Vector2Int linePos = new Vector2Int(int.Parse(line.Split('*')[0].Split(',')[0]), int.Parse(line.Split('*')[0].Split(',')[1]));
                string lineData = line.Split('*')[1] + "*" + line.Split('*')[2];

                if (!blockChanges.ContainsKey(linePos))
                    blockChanges.Add(linePos, lineData);
            }

            //Empty lines before writing
            File.WriteAllText(path + "\\blocks", "");

            TextWriter c = new StreamWriter(path + "\\blocks");

            foreach (KeyValuePair<Vector2Int, string> line in blockChanges)
            {

                c.WriteLine(line.Key.x + "," + line.Key.y + "*" + line.Value);
            }

            c.Close();
            blockChanges.Clear();
        }

        //Save Entities
        foreach (Entity e in GetEntities())
        {
            e.Save();
        }
    }

    public void UnloadChunk()
    {
        if (isSpawnChunk)
            return;

        if (age >= MinimumUnloadTime*TickRate)
        {
            Save();
            WorldManager.instance.chunks.Remove(ChunkPosition);
            Destroy(gameObject);
        }
    }

    public bool isBlockLocal(Vector2Int worldPos)
    {
        //(ChunkPosition == GetChunkPosFromWorldPosition(worldPos.x));
        bool local = (GetChunkPosFromWorldPosition(worldPos.x) == ChunkPosition);
        
        if (worldPos.y < 0 || worldPos.y > Height)
            local = false;
        /*
        if (worldPos.y % 10 == 1)
            print(worldPos.x + "/16 == " + GetChunkPosFromWorldPosition(worldPos.x) + " =? " + ChunkPosition + ", loocal="+local);
        if ((GetChunkPosFromWorldPosition(worldPos.x) == ChunkPosition) != local)
            Debug.LogError(worldPos.x + "/16 == " + GetChunkPosFromWorldPosition(worldPos.x) + " =? " + ChunkPosition + ", loocal=" + local);
           */
        return local;
    }

    public static Block setBlock(Vector2Int worldPos, Material mat)
    {
        return setBlock(worldPos, mat, "", true);
    }

    public static Block setBlock(Vector2Int worldPos, Material mat, bool save)
    {
        return setBlock(worldPos, mat, "", save);
    }
    
    public static Block setBlock(Vector2Int worldPos, Material mat, string data, bool save)
    {
        return setBlock(worldPos, mat, data, save, true);
    }

    public static Block setBlock(Vector2Int worldPos, Material mat, string data, bool save, bool spreadTick)
    {
        Chunk chunk = GetChunk(GetChunkPosFromWorldPosition((int)worldPos.x));

        if (chunk == null)
            return null;

        return chunk.setLocalBlock(worldPos, mat, data, save, spreadTick);
    }


    public Block setLocalBlock(Vector2Int worldPos, Material mat)
    {
        return setLocalBlock(worldPos, mat, "", true);
    }

    public Block setLocalBlock(Vector2Int worldPos, Material mat, bool save)
    {
        return setLocalBlock(worldPos, mat, "", save);
    }

    public Block setLocalBlock(Vector2Int worldPos, Material mat, string data, bool save)
    {
        return setLocalBlock(worldPos, mat, data, save, true);
    }

    public Block setLocalBlock(Vector2Int worldPos, Material mat, string data, bool save, bool spreadTick)
    {
        System.Type type = System.Type.GetType(mat.ToString());
        if (!type.IsSubclassOf(typeof(Block)))
            return null;

        if (!isLoaded && age > 0)
        {
            StartCoroutine(ScheduleSetBlock(1, worldPos, mat, data, save));
            return null;
        }

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
            

            return block.GetComponent<Block>();
        }
        return null;
    }

    IEnumerator ScheduleSetBlock(float delay, Vector2Int worldPos, Material mat, string data, bool save)
    {
        yield return new WaitForSecondsRealtime(delay);
        setLocalBlock(worldPos, mat, data, save);
    }

    public static Block getBlock(Vector2Int worldPos)
    {
        Chunk chunk = GetChunk(GetChunkPosFromWorldPosition((int)worldPos.x));

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

        if (!blocks.ContainsKey(worldPos))
            return null;

        Block block = blocks[worldPos];

        if (block == null)
            return null;

        return block;
    }

    public Block loadTheoreticalBlock(Vector2Int worldPos)
    {
        Block block = null;
        Material mat = Material.Air;


        if (mat == Material.Air)
            mat = getTheoreticalTerrainBlock(worldPos);
        if (mat == Material.Air)
            return null;

        block = setLocalBlock(worldPos, mat, "", false, false);

        return block;
    }
    
    LibNoise.Generator.Perlin caveNoise;
    LibNoise.Generator.Perlin patchNoise;
    LibNoise.Generator.Perlin lakeNoise;
    public Material getTheoreticalTerrainBlock(Vector2Int worldPos)
    {
        System.Random r = new System.Random(seedByPosition(worldPos));
        Material mat = Material.Air;
        List<Biome> biomes = getBiomes(worldPos.x);
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
        foreach (Entity e in GameObject.FindObjectsOfType<Entity>())
        {
            if (e.transform.position.x >= chunkXPosition && e.transform.position.x <= (chunkXPosition + Width))
                entities.Add(e);
        }

        return entities.ToArray();
    }

    public Block getTopmostBlock(int x)
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
        return new System.Random((WorldManager.world.seed.ToString() + ", " + pos.x + ", " + pos.y)
            .GetHashCode()).Next(int.MinValue, int.MaxValue);
    }

    public static Biome getMostProminantBiome(int pos)
    {
        return getBiomes(pos)[0];
    }

    public static List<Biome> getBiomes(int pos)
    {
        List<Biome> biomes = new List<Biome>();
        WorldManager.instance.biomes.ForEach((item) =>
        {
            biomes.Add(item);
        });

        biomes.Sort((x, y) => x.getBiomeValueAt(pos).CompareTo(y.getBiomeValueAt(pos)));

        return biomes;
    }
}