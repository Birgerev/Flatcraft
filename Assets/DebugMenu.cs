using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class DebugMenu : MonoBehaviour
{
    public static bool active = false;

    public Text text_fps;
    public Text text_entityCount;
    public Text text_x;
    public Text text_y;
    public Text text_dimension;    
    public Text text_biome;
    public Text text_seed;
    public Text text_time;

    private float deltaTime = 0.0f;

    // Update is called once per frame
    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        updateState();

        if(active)
            updateText();
    }

    private void updateState()
    {
        if (Input.GetKeyDown(KeyCode.F3))
            active = !active;

        GetComponent<CanvasGroup>().alpha = (active) ? 1 : 0;
        GetComponent<CanvasGroup>().interactable = (active);
        GetComponent<CanvasGroup>().blocksRaycasts = (active);
    }

    private void updateText()
    {
        text_fps.text = "fps: " + (int)(1.0f / deltaTime);

        text_entityCount.text = "entity count: " + Entity.entityCount + ",  living: "+Entity.livingEntityCount;

        Player player = Player.localInstance;
        text_x.text = "x: " + player.transform.position.x;
        text_y.text = "y: " + player.transform.position.y;
        text_dimension.text = "dimension: " + player.location.dimension.ToString();
        Biome biome = Chunk.getBiome((int)player.transform.position.x, player.location.dimension);
        if(biome != null)
            text_biome.text = "biome: " + biome.name;

        text_seed.text = "seed: " + WorldManager.world.seed;
        text_time.text = "time: " + (int)WorldManager.world.time + ", (day "+ (int)(WorldManager.world.time/WorldManager.dayLength) + ")";
    }
}
