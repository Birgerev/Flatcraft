using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public static World world;
    public static WorldManager instance;

    public GameObject chunkPrefab;

    public string loadingState = "";
    public float loadingProgress = 0;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        StartCoroutine(LoadWorld());
    }


    IEnumerator LoadWorld()
    {
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Game"));
        float steps = 6;

        loadingState = "Generating Spawn Chunks";
        loadingProgress = 1f / steps;
        //Load Chunk [0]
        Chunk mainChunk = Chunk.LoadChunk(0);

        while (!mainChunk.isLoaded)
        {
            yield return new WaitForSeconds(0.5f);
        }
        
        loadingState = "Generating Chunks";
        loadingProgress = 2f / steps;

        while (Chunk.amountOfChunksLoading > 0)
        {
            yield return new WaitForSeconds(0.5f);
        }

        loadingState = "Ticking Chunks";
        loadingProgress = 3f / steps;

        yield return new WaitForSeconds(1f);

        loadingState = "Loading Player";
        loadingProgress = 4f / steps;

        yield return new WaitForSeconds(1f);

        loadingState = "Done!";
        loadingProgress = 5f / steps;

        yield return new WaitForSeconds(0.3f);

        loadingProgress = 6f / steps;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
