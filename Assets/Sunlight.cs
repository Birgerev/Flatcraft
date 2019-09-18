using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class Sunlight : MonoBehaviour
{
    public int lightFidelity;

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
            yield return new WaitForSeconds(5);
            UpdateLight();
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
        
        Light2D light = GetComponent<Light2D>();
        light.m_ShapePath = lightPoints.ToArray();
    }
}
