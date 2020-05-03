using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugBiomeLandscape : MonoBehaviour
{
    [Space]
    public Biome biome;

    [Space]
    public int previewWidth;
    public float previewUpdateFrequency;
    public bool showSeaLevel;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(UpdateLoop());
    }

    IEnumerator UpdateLoop()
    {
        while (true)
        {
            Texture2D tex = new Texture2D(previewWidth, Chunk.Height);
            
            for(int x = 0; x < previewWidth; x++)
            {
                for (int y = 0; y < Chunk.Height; y++)
                {
                    float noiseValue = biome.getLandscapeNoiseAt(new Location(x, y));
                    if (noiseValue > 0.1f)
                    {
                        tex.SetPixel(x, y, Color.black);
                    }
                }
            }
            
            if(showSeaLevel)
                for (int x = 0; x < previewWidth; x++)
                    tex.SetPixel(x, Chunk.SeaLevel, Color.red);
            
            tex.Apply();
            
            Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), Vector2.zero);
            GetComponent<SpriteRenderer>().sprite = sprite;
            
            yield return new WaitForSeconds(previewUpdateFrequency);
        }
    }
}
