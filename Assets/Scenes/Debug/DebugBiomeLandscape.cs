using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugBiomeLandscape : MonoBehaviour
{
    public Dictionary<Vector2, GameObject> blocks = new Dictionary<Vector2, GameObject>();

    public GameObject block;

    [Space]
    public Biome biome;

    [Space]
    public int previewSize;
    public float previewUpdateFrequency;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(UpdateLoop());
    }

    IEnumerator UpdateLoop()
    {
        while (true)
        {
            for(int x = 0; x < previewSize; x++)
            {
                for (int y = 0; y < Chunk.Height; y++)
                {
                    bool exists = blocks.ContainsKey(new Vector2(x, y));

                    float noiseValue = biome.getLandscapeNoiseAt(new Location(x, y));
                    if (noiseValue > 0.1f)
                    {
                        if (!exists)
                        {
                            GameObject obj = Instantiate(block);

                            obj.transform.SetParent(transform);
                            obj.transform.position = new Vector3(x, y);
                            blocks[new Vector2(x, y)] = obj;
                        }
                    }
                    else if (exists)
                    {
                        Destroy(blocks[new Vector2(x, y)]);
                        blocks.Remove(new Vector2(x, y));
                    }
                }
                yield return new WaitForSeconds(previewUpdateFrequency / previewSize);
            }
        }
    }
}
