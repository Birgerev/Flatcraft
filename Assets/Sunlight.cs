using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sunlight : MonoBehaviour
{
    private int lightFidelity = 5;
    private float updateTime = 2f;
    public Color sunlightColor = Color.white;
    public bool loaded = false;

    public static Sunlight instance;

    /*
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    public void BeginGenerating()
    {
        StartCoroutine(UpdateLight());
    }

    IEnumerator UpdateLight()
    {
        while (true)
        {
            SortedDictionary<int, Chunk> sortedChunks = new SortedDictionary<int, Chunk>(WorldManager.instance.chunks);
            List<Vector3> lightPoints = new List<Vector3>();

            if (sortedChunks.Count == 0)
            {
                yield return new WaitForSeconds(updateTime);
                continue;
            }

            int minX = int.MaxValue;
            int maxX = int.MinValue;

            int blockCount = sortedChunks.Count * (Chunk.Width/lightFidelity);
            
            foreach (KeyValuePair<int, Chunk> chunk in sortedChunks)
            {
                for (int i = chunk.Value.ChunkPosition * Chunk.Width; i < (chunk.Value.ChunkPosition * Chunk.Width) + Chunk.Width; i += lightFidelity)
                {
                    if (chunk.Value == null)
                        continue;
                    if (chunk.Value.getTopmostBlock(i) == null)
                        continue;

                    lightPoints.Add(new Vector3(i, chunk.Value.getTopmostBlock(i).position.y));

                    if (i > maxX)
                        maxX = i;
                    if (i < minX)
                        minX = i;

                    yield return new WaitForSeconds((updateTime/ blockCount) / 3);
                }
            }

            lightPoints.Add(new Vector3(maxX, Chunk.Height));
            lightPoints.Add(new Vector3(minX, Chunk.Height));

            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).gameObject.name == "old")
                {
                    Destroy(transform.GetChild(i).gameObject);
                    yield return new WaitForSeconds((updateTime / blockCount) / 3);
                }
            }

            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.name = "old";
            }
            
            foreach (Vector3 pos in lightPoints)
            {
                GameObject lightObj = Instantiate((GameObject)Resources.Load("Objects/BlockLight"));
                lightObj.transform.SetParent(transform);
                lightObj.transform.localPosition = pos;
                lightObj.transform.name = "_light";

                BlockLight light = lightObj.GetComponent<BlockLight>();
                light.color = sunlightColor;
                light.glowingLevel = 17;
                light.sunlight = true;

                yield return new WaitForSeconds((updateTime / blockCount) / 3);
            }

            if (!loaded)
                loaded = true;
        }
    }*/
}
