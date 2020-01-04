using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public static World world;
    public static WorldManager instance;

    public List<Biome> biomes = new List<Biome>();

    public Dictionary<int, Chunk> chunks = new Dictionary<int, Chunk>();
    public int amountOfChunksLoading = 0;

    public GameObject chunkPrefab;
    public GameObject player;

    public string loadingState = "";
    public float loadingProgress = 0;

    public Chunk mainChunk;

    public static float dayLength = 60 * 20;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        StartCoroutine(LoadWorld());
    }

    private void Update()
    {
        world.time += Time.deltaTime;
    }

    IEnumerator SaveLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Chunk.AutosaveDuration);
            world.SaveData();
        }
    }

    IEnumerator LoadWorld()
    {
        Time.timeScale = 1;
        chunks.Clear();

        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Game"));
        float steps = 5;

        loadingState = "Loading Player";
        loadingProgress = 1f / steps;

        Spawn();

        yield return new WaitForSeconds(1f);

        loadingState = "Generating Chunks: 0";
        loadingProgress = 2f / steps;
        //Load Chunk [0]
        mainChunk = Chunk.LoadChunk(0);

        for(int i = -Chunk.RenderDistance; i < Chunk.RenderDistance; i++)
        {
            Chunk.LoadChunk((int)((float)Player.localInstance.transform.position.x/(float)Chunk.Width) + i);

            print("index " + i + " loading " + ((int)((float)Player.localInstance.transform.position.x / (float)Chunk.Width) + i));
        }

        while (amountOfChunksLoading > 0 || chunks.Count < 3)
        {
            loadingState = "Generating Chunks: "+ amountOfChunksLoading;
            yield return new WaitForSeconds(0.5f);
        }

        loadingState = "Waiting For Light";
        loadingProgress = 3f / steps;
        
        yield return new WaitForSeconds(1f);

        loadingState = "Done!";
        loadingProgress = 4f / steps;

        yield return new WaitForSeconds(0.2f);

        loadingProgress = 5f / steps;

        StartCoroutine(SaveLoop());
    }

    public void Spawn()
    {
        GameObject obj = Instantiate(player);
    }
}
