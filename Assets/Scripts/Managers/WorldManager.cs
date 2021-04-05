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
    
    public HashSet<Location> caveHollowBlocks = new HashSet<Location>();
    public List<int> caveGeneratedRegions = new List<int>();

    public List<Biome> overworldBiomes = new List<Biome>();
    public Biome netherBiome;
    public Dictionary<ChunkPosition, Biome> chunkBiomes = new Dictionary<ChunkPosition, Biome>();

    public GameObject chunkPrefab;

    public Dictionary<ChunkPosition, Chunk> chunks = new Dictionary<ChunkPosition, Chunk>();

    public static World world;
    [SyncVar]
    public float worldTime;
    
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
            if(isServer)
                world.SaveData();
        }
    }
}