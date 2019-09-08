using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibNoise;
using System.IO;

public class Chunk : MonoBehaviour
{
    public static Dictionary<int, Chunk> chunks = new Dictionary<int, Chunk>();
    public static int amountOfChunksLoading = 0;

    public static float AutosaveDuration = 1;
    public static int Width = 16, Height = 255;
    public static int RenderDistance = 1;
    public static int SpawnChunkDistance = 0;
    public static int MinimumUnloadTime = 10;
    public static int TickRate = 1;

    public GameObject blockPrefab;

    public int ChunkPosition = 0;
    public bool isSpawnChunk = false;
    public bool isTickedChunk = false;
    public bool isLoaded = false;
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
    public static double ore_diamond_chance = 0.001f;

    public static int lava_height = 10;
    public static int sea_level = 62;


    public Dictionary<Vector2Int, string> blockChanges = new Dictionary<Vector2Int, string>();

    private void Start()
    {
        if (chunks.ContainsKey(ChunkPosition))
        {
            Debug.LogWarning("A duplicate of Chunk ["+ChunkPosition+"] has been destroyed.");
            Destroy(gameObject);
            return;
        }

        chunks.Add(ChunkPosition, this);

        gameObject.name = "Chunk [" + ChunkPosition + "]";
        transform.position = new Vector3(ChunkPosition * Width, 0, 0);

        StartCoroutine(GenerateChunk());
        StartCoroutine(SaveLoop());
    }

    private void OnDestroy()
    {
        if(isLoaded)
            chunks.Remove(ChunkPosition);
    }

    private void Update()
    {
        isSpawnChunk = (ChunkPosition <= SpawnChunkDistance || ChunkPosition >= -SpawnChunkDistance);
    }

    public static Chunk GetChunk(int cPos)
    {
        Chunk chunk = null;
        
        if(chunks.ContainsKey(cPos))
            chunk = chunks[cPos];
        if(chunk == null)
        {
            if(cPos != 0)
                chunk = LoadChunk(cPos);
        }

        return chunk;
    }
    
    public static Chunk GetChunkFromWorldPosition(int worldPos)
    {
        Chunk chunk = null;
        if (worldPos >= 0)
        {
            chunk = GetChunk(Mathf.FloorToInt(worldPos / (Width)));
        }
        else
        {
            chunk = GetChunk(Mathf.CeilToInt((worldPos+1) / Width) - 1);
        }

        return chunk;
    }

    public static Chunk LoadChunk(int cPos)
    {
        GameObject newChunk = Instantiate(WorldManager.instance.chunkPrefab);

        newChunk.GetComponent<Chunk>().ChunkPosition = cPos;

        return newChunk.GetComponent<Chunk>();
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

            isTickedChunk = inRenderDistance();

            //Tick Blocks
            //Update neighbor chunks
            if (isTickedChunk)
            {
                Block[] blocks = transform.GetComponentsInChildren<Block>();
                float timePerBlock = (1 / TickRate) / blocks.Length;

                foreach (Block block in blocks)
                {
                    block.Tick();
                }
            }

            age++;

            if(!inRenderDistance())
            {
                UnloadChunk();
            }
            yield return new WaitForSeconds(1 / TickRate);
        }
    }

    public bool inRenderDistance()
    {
        return inRenderDistance(ChunkPosition);
    }

    public bool inRenderDistance(int cPos)
    {
        cPos ++;

        Vector2 playerPosition;


        if (Player.localInstance == null)
            playerPosition = Vector2.zero;
        else
            playerPosition = Player.localInstance.transform.position;

        int chunkXPosition = cPos * (int)Width;
        float distanceFromPlayer = Mathf.Abs((cPos * Width) - playerPosition.x);

        return distanceFromPlayer < RenderDistance * Width;
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
        noise = new LibNoise.Generator.Perlin(1f, 1f, 1f, 2, WorldManager.world.seed, QualityMode.Low);
        patchNoise = new LibNoise.Generator.Perlin(0.6f, 0.8f, 0.8f, 2, WorldManager.world.seed, QualityMode.Low);
        lakeNoise = new LibNoise.Generator.Perlin(2, 0.8f, 5f, 2, WorldManager.world.seed, QualityMode.Low);
        caveNoise = new LibNoise.Generator.Perlin(caveFrequency, caveLacunarity, cavePercistance, caveOctaves, WorldManager.world.seed, QualityMode.High);

        amountOfChunksLoading++;
        for (int y = 0; y <= Height; y++)
        {
            int blocksGenerated = 0;
            for (int x = 0; x < Width; x++)
            {
                Block loadedBlock = loadTheoreticalBlock(new Vector2Int(x, y) + Vector2Int.CeilToInt(transform.position));

                if (loadedBlock != null)
                    blocksGenerated++;
            }
            if(blocksGenerated > 0)
                yield return new WaitForSeconds(0.005f * (amountOfChunksLoading));
        }

        StartCoroutine(Tick());

        yield return new WaitForSeconds(0.5f);

        //Load
        string path = WorldManager.world.getPath() + "\\region\\" + ChunkPosition;
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

        isLoaded = true;
        amountOfChunksLoading--;
    }

    IEnumerator SaveLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(AutosaveDuration);
            Save();
        }
    }

    public void Save()
    {
        if (blockChanges.Count > 0)
        {
            string path = WorldManager.world.getPath() + "\\region\\" + ChunkPosition;
            if (!File.Exists(path))
                File.Create(path).Close();

            foreach (string line in File.ReadAllLines(path))
            {
                Vector2Int linePos = new Vector2Int(int.Parse(line.Split('*')[0].Split(',')[0]), int.Parse(line.Split('*')[0].Split(',')[1]));
                string lineData = line.Split('*')[1] + "*" + line.Split('*')[2];

                if (!blockChanges.ContainsKey(linePos))
                    blockChanges.Add(linePos, lineData);
            }

            //Empty lines before writing
            File.WriteAllText(path, "");

            TextWriter c = new StreamWriter(path);

            foreach (KeyValuePair<Vector2Int, string> line in blockChanges)
            {
                c.WriteLine(line.Key.x + "," + line.Key.y + "*" + line.Value);
            }

            c.Close();
            blockChanges.Clear();
        }
    }

    public void UnloadChunk()
    {
        if (isSpawnChunk)
            return;

        Save();
        if (age >= MinimumUnloadTime*TickRate)
        {
            chunks.Remove(ChunkPosition);
            Destroy(gameObject);
        }
    }

    public bool isBlockLocal(Vector2Int worldPos)
    {
        bool local = true;

        if (worldPos.x < transform.position.x)
            local = false;

        if (worldPos.x > (transform.position.x + (Width - 1)))
        {
            if (worldPos.x < 0)
            {
                if(worldPos.x > (transform.position.x + (Width + 1)))
                    local = false;
            }
            else local = false;
        }
        if (worldPos.y < 0 || worldPos.y > Height)
            local = false;

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
        Chunk chunk = GetChunkFromWorldPosition((int)worldPos.x);

        return chunk.setLocalBlock(worldPos, mat, data, save);
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

        if (mat != Material.Air)
        {
            //Place new block
            GameObject block = null;

            block = Instantiate(blockPrefab);

            //Attach it to the object
            block.AddComponent(type);

            block.transform.parent = transform;
            block.transform.position = (Vector2)worldPos;

            blocks.Add(worldPos, block.GetComponent<Block>());

            block.GetComponent<Block>().data = Block.dataFromString(data);

            return block.GetComponent<Block>();
        }
        return null;
    }

    IEnumerator ScheduleSetBlock(float delay, Vector2Int worldPos, Material mat, string data, bool save)
    {
        yield return new WaitForSeconds(delay);
        setLocalBlock(worldPos, mat, data, save);
    }

    public static Block getBlock(Vector2Int worldPos)
    {
        Chunk chunk = GetChunkFromWorldPosition((int)worldPos.x);
        Block block = chunk.getLocalBlock(worldPos);

        return block;
    }

    public Block getLocalBlock(Vector2Int worldPos)
    {
        if (!isBlockLocal(worldPos))
        {
            Debug.LogWarning("Tried setting local block outside of chunk (" + worldPos.x + ", " + worldPos.y + ") inside Chunk [" + ChunkPosition + "]");
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

        block = setLocalBlock(worldPos, mat, false);

        return block;
    }
    
    LibNoise.Generator.Perlin noise;
    LibNoise.Generator.Perlin caveNoise;
    LibNoise.Generator.Perlin patchNoise;
    LibNoise.Generator.Perlin lakeNoise;
    public Material getTheoreticalTerrainBlock(Vector2Int worldPos)
    {
        System.Random r = new System.Random(seedByPosition(worldPos));
        Material mat = Material.Air;


                    //-Terrain Generation-//
        double noiseValue = 
            noise.GetValue((float)worldPos.x / 20, (float)worldPos.y / 20) + 4.0f;
        double density = 1;

        if(worldPos.y > sea_level-10)
        {
            float heightWheight = 0.08f;
            density = noiseValue - (heightWheight * ((float)worldPos.y - (sea_level-43)));
        }

        if (density > 0.1f)
        {
            mat = Material.Grass;

            if (density > 0.5f)
            {
                mat = Material.Stone;
                
                if(Mathf.Abs((float)caveNoise.GetValue((float)worldPos.x / 20, (float)worldPos.y / 20)) > 7.5f)
                {
                    mat = Material.Dirt;
                }
                if (Mathf.Abs((float)caveNoise.GetValue((float)worldPos.x / 20 + 100, (float)worldPos.y / 20, 200)) > 7.5f)
                {
                    mat = Material.Gravel;
                }
            }
        }

        if (mat == Material.Air && worldPos.y <= sea_level)
        {
            mat = Material.Water;
        }

        if(density > 0.1f)
        {
            double caveValue =
                (caveNoise.GetValue((float)worldPos.x / 20, (float)worldPos.y / 20) + 4.0f) / 4f;
            if (caveValue > caveHollowValue)
            {
                mat = Material.Air;

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

    public static int seedByPosition(Vector2Int pos)
    {
        return new System.Random((WorldManager.world.seed.ToString() + ", " + pos.x + ", " + pos.y)
            .GetHashCode()).Next(int.MinValue, int.MaxValue);
    }
}