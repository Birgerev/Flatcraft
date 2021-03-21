using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldManager : NetworkBehaviour
{
    public static WorldManager instance;

    public static float dayLength = 60 * 20;
    public int amountOfChunksLoading;
    
    public HashSet<Location> caveHollowBlocks = new HashSet<Location>();
    public List<int> caveGeneratedRegions = new List<int>();

    public List<Biome> overworldBiomes = new List<Biome>();
    public Biome netherBiome;
    public Dictionary<ChunkPosition, Biome> chunkBiomes = new Dictionary<ChunkPosition, Biome>();

    public GameObject chunkPrefab;
    public GameObject playerPrefab;

    public Dictionary<ChunkPosition, Chunk> chunks = new Dictionary<ChunkPosition, Chunk>();
    public float loadingProgress;

    public string loadingState = "";

    public static World world;
    [SyncVar]
    public float worldTime;
    
    // Start is called before the first frame update
    private void Start()
    {
        instance = this;
        
        StartCoroutine(LoadWorld());
    }

    private void Update()
    {
        if (isServer)
        {
            world.time += Time.deltaTime;
            
            if ((Time.time % 0.5f) - Time.deltaTime <= 0)
            {
                worldTime = world.time;
            }
        }
    }

    private IEnumerator SaveLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(SaveManager.AutosaveDuration);
            world.SaveData();
        }
    }

    private IEnumerator LoadWorld()
    {
        SeedGenerator.Reset();
        Entity.entities.Clear();
        Time.timeScale = 1;
        chunks.Clear();
        
        //SceneManager.SetActiveScene(SceneManager.GetSceneByName("Game"));
        float steps = 5;
        /*
        loadingState = "Loading Player";
        loadingProgress = 1f / steps;
        
        yield return new WaitForSeconds(1f);

        
        while (amountOfChunksLoading == 0 || chunks.Count == 0)
        {
            loadingState = "Creating Chunks";
            yield return new WaitForSeconds(0.5f);
        }

        loadingState = "Generating Spawn Chunk: 0";
        loadingProgress = 2f / steps;

        while (amountOfChunksLoading > 0 || chunks.Count < 3)
        {
            loadingState = "Building Terrain: " + amountOfChunksLoading + " Chunks Left";
            yield return new WaitForSeconds(0.5f);
        }

        loadingState = "Waiting For Light";
        loadingProgress = 3f / steps;

        yield return new WaitForSeconds(1f);

        loadingState = "Done!";
        loadingProgress = 4f / steps;

        yield return new WaitForSeconds(0.2f);*/

        loadingProgress = 5f / steps;
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(SaveLoop());
    }
}