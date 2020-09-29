using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldManager : MonoBehaviour
{
    public static World world;
    public static WorldManager instance;

    public static float dayLength = 60 * 20;
    public int amountOfChunksLoading;

    public List<Biome> biomes = new List<Biome>();
    public Dictionary<ChunkPosition, Biome> chunkBiomes = new Dictionary<ChunkPosition, Biome>();

    public GameObject chunkPrefab;

    public Dictionary<ChunkPosition, Chunk> chunks = new Dictionary<ChunkPosition, Chunk>();
    public float loadingProgress;

    public string loadingState = "";

    public Chunk mainChunk;
    public GameObject player;

    // Start is called before the first frame update
    private void Start()
    {
        instance = this;
        StartCoroutine(LoadWorld());
    }

    private void Update()
    {
        world.time += Time.deltaTime;
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
        Time.timeScale = 1;
        chunks.Clear();

        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Game"));
        float steps = 5;

        loadingState = "Loading Player";
        loadingProgress = 1f / steps;

        Spawn();

        yield return new WaitForSeconds(1f);

        loadingState = "Generating Spawn Chunk: 0";
        loadingProgress = 2f / steps;
        //Load Chunk [0]
        mainChunk = new ChunkPosition(0, Dimension.Overworld).CreateChunk();

        while (amountOfChunksLoading > 0 || chunks.Count < 3)
        {
            loadingState = "Generating Chunks: " + amountOfChunksLoading;
            yield return new WaitForSeconds(0.5f);
        }

        loadingState = "Waiting For Light";
        loadingProgress = 3f / steps;

        yield return new WaitForSeconds(1f);

        loadingState = "Done!";
        loadingProgress = 4f / steps;

        yield return new WaitForSeconds(0.2f);

        loadingProgress = 5f / steps;


        if (Player.localInstance.location.x == 0) //Place player at ground first time player spawns
            Player.localInstance.transform.position =
                Player.localInstance.ValidSpawn(Player.localInstance.location.x).GetPosition();

        StartCoroutine(SaveLoop());
    }

    public void Spawn()
    {
        var obj = Instantiate(player);
    }
}