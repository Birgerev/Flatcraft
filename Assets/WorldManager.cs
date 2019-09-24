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

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        StartCoroutine(LoadWorld());
    }

    private void Update()
    {
        foreach(Biome biome in biomes)
        {
            biome.GenerateCurves();
        }
    }

    IEnumerator LoadWorld()
    {
        Time.timeScale = 1;
        chunks.Clear();

        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Game"));
        float steps = 6;

        loadingState = "Loading Player";
        loadingProgress = 1f / steps;

        Spawn();

        yield return new WaitForSeconds(1f);

        loadingState = "Generating Spawn Chunks";
        loadingProgress = 2f / steps;
        //Load Chunk [0]
        mainChunk = Chunk.LoadChunk(0);

        while (!mainChunk.isLoaded)
        {
            yield return new WaitForSeconds(0.5f);
        }
        
        loadingState = "Generating Chunks";
        loadingProgress = 3f / steps;

        while (amountOfChunksLoading > 0)
        {
            yield return new WaitForSeconds(0.5f);
        }

        loadingState = "Ticking Chunks";
        loadingProgress = 4f / steps;

        yield return new WaitForSeconds(1f);

        loadingState = "Done!";
        loadingProgress = 5f / steps;

        yield return new WaitForSeconds(0.3f);

        loadingProgress = 6f / steps;
    }

    public void Spawn()
    {
        GameObject obj = Instantiate(player);

        obj.GetComponent<Player>().Spawn();
    }
}
