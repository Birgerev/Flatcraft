using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class WorldManager : NetworkBehaviour
{
    public static WorldManager instance;
    public static World world = new World("multiplayerUnassigned", 0);

    public const float DayLength = 60 * 20;

    public HashSet<Location> caveHollowBlocks = new HashSet<Location>();
    public List<int> caveGeneratedRegions = new List<int>();

    public List<Biome> overworldBiomes = new List<Biome>();
    public Biome netherBiome;

    public GameObject chunkPrefab;

    public Hashtable chunks = new Hashtable();

    [SyncVar] public float worldTime;

    public Dictionary<ChunkPosition, Biome> chunkBiomes = new Dictionary<ChunkPosition, Biome>();

    public Dictionary<int, Inventory> loadedInventories = new Dictionary<int, Inventory>();
    public List<SignEditMenu> openSignMenus = new List<SignEditMenu>();

    // Start is called before the first frame update
    private void Start()
    {
        instance = this;
        Entity.entities.Clear();
        Time.timeScale = 1;
        chunks.Clear();
        //Update to current version
        world.versionId = Version.currentId;
            
        StartCoroutine(SaveLoop());
    }

    private void Update()
    {
        if (isServer)
        {
            //Increment world time
            world.time += Time.deltaTime;

            //Update world time (forces sync as well)
            if (Time.time % 0.5f - Time.deltaTime <= 0)
                worldTime = world.time;
        }
    }
    
    public static TimeOfDay GetTimeOfDay()
    {
        return instance.worldTime % DayLength > DayLength / 2
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