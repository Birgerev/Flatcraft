using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class Sunlight : MonoBehaviour
{
    public int lightFidelity;
    public Color sunlightColor = Color.white;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Loop());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Loop()
    {
        while (true)
        {
            yield return new WaitForSeconds(2);
            try
            {
                UpdateLight();
            }catch(System.Exception e)
            {
                Debug.LogError(e);
            }
        }
    }

    public void UpdateLight()
    {
        SortedDictionary<int, Chunk> sortedChunks = new SortedDictionary<int, Chunk>(WorldManager.instance.chunks);
        List<Vector3> lightPoints = new List<Vector3>();

        int minX = int.MaxValue;
        int maxX = int.MinValue;

        foreach (KeyValuePair<int, Chunk> chunk in sortedChunks)
        {
            for(int i = chunk.Value.ChunkPosition*Chunk.Width; i < (chunk.Value.ChunkPosition * Chunk.Width) + Chunk.Width; i += lightFidelity)
            {
                if (chunk.Value.getTopmostBlock(i) == null)
                    continue;

                lightPoints.Add(new Vector3(i, chunk.Value.getTopmostBlock(i).getPosition().y));

                if (i > maxX)
                    maxX = i;
                if (i < minX)
                    minX = i;
            }
        }

        lightPoints.Add(new Vector3(maxX, Chunk.Height));
        lightPoints.Add(new Vector3(minX, Chunk.Height));

        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.name == "old")
                Destroy(transform.GetChild(i).gameObject);
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
        }
    }
}
