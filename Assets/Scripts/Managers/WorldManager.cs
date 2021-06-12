using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class WorldManager : NetworkBehaviour
{
    public static WorldManager instance;

    public static float dayLength = 60 * 20;

    public static World world;

    public HashSet<Location> caveHollowBlocks = new HashSet<Location>();
    public List<int> caveGeneratedRegions = new List<int>();

    public List<Biome> overworldBiomes = new List<Biome>();
    public Biome netherBiome;

    public GameObject chunkPrefab;

    public Hashtable chunks = new Hashtable();

    [SyncVar] public float worldTime;

    public Dictionary<ChunkPosition, Biome> chunkBiomes = new Dictionary<ChunkPosition, Biome>();

    public Dictionary<int, Inventory> loadedInventories = new Dictionary<int, Inventory>();

    // Start is called before the first frame update
    private void Start()
    {
        instance = this;
        Entity.entities.Clear();
        Time.timeScale = 1;
        chunks.Clear();
        StartCoroutine(SaveLoop());
    }

    private void Update()
    {
        if (isServer)
        {
            world.time += Time.deltaTime;

            if (Time.time % 0.5f - Time.deltaTime <= 0)
                worldTime = world.time;
        }
    }
    
    public static TimeOfDay GetTimeOfDay()
    {
        return instance.worldTime % dayLength > dayLength / 2
            ? TimeOfDay.Night
            : TimeOfDay.Day;
    }

    private IEnumerator SaveLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(SaveManager.AutosaveDuration);
            if (isServer)
                world.SaveData();
        }
    }
}