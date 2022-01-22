using System.Collections;
using UnityEngine;

public class DebugBiomeLandscape : MonoBehaviour
{
    [Space] public Biome biome;

    public float previewUpdateFrequency;

    [Space] public int previewWidth;

    public bool showSeaLevel;
    public bool showStoneLayer;

    // Start is called before the first frame update
    private void Start()
    {
        StartCoroutine(UpdateLoop());
    }

    private IEnumerator UpdateLoop()
    {
        while (true)
        {
            Texture2D tex = new Texture2D(previewWidth, Chunk.Height);

            for (int x = 0; x < previewWidth; x++)
            for (int y = 0; y < Chunk.Height; y++)
            {
                float noiseValue = biome.GetLandscapeNoiseAt(new Location(x, y));
                if (noiseValue > 0.1f)
                {
                    bool isStone = noiseValue > biome.stoneLayerNoiseValue;

                    tex.SetPixel(x, y, showStoneLayer && isStone ? Color.black : Color.gray);
                }
            }

            if (showSeaLevel)
                for (int x = 0; x < previewWidth; x++)
                    tex.SetPixel(x, OverworldGenerator.SeaLevel, Color.red);

            tex.Apply();

            Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), Vector2.zero);
            GetComponent<SpriteRenderer>().sprite = sprite;

            yield return new WaitForSeconds(previewUpdateFrequency);
        }
    }
}