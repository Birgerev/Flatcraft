using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugBiomeWave : MonoBehaviour
{
    public Dictionary<int, GameObject> blocks = new Dictionary<int, GameObject>();

    public GameObject block;

    [Space]
    public Biome biome;

    [Space]
    public int previewSize;
    public float previewUpdateFrequency;
    public float startHeight;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(UpdateLoop());
    }

    IEnumerator UpdateLoop()
    {
        while (true)
        {
            for (int x = 0; x < previewSize; x++)
            {
                GameObject obj = null;
                if (blocks.ContainsKey(x))
                    obj = (blocks[x]);

                float noiseValue = biome.getBiomeValueAt(x);

                if (obj == null)
                {
                    obj = Instantiate(block);
                    yield return new WaitForSeconds(previewUpdateFrequency / previewSize);
                }

                obj.transform.SetParent(transform);
                obj.transform.position = new Vector3(x, startHeight);
                obj.transform.localScale = new Vector3(1, startHeight + noiseValue);
                blocks[x] = obj;

            }
            yield return new WaitForSeconds(previewUpdateFrequency);
        }
    }
}
