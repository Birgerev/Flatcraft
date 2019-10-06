using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class DebugMenu : MonoBehaviour
{
    public static bool active = false;

    public Text text_fps;
    public Text text_x;
    public Text text_y;
    public Text text_biome;
    public Text text_seed;
    public Text text_time;

    private float deltaTime = 0.0f;

    // Update is called once per frame
    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        updateState();
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
        text_fps.text = "fps: " + (1.0f / deltaTime);

        Player player = Player.localInstance;
        text_x.text = "x: " + player.transform.position.x;
        text_y.text = "y: " + player.transform.position.y;
        text_biome.text = "biome: " + Chunk.getMostProminantBiome((int)player.transform.position.x).name;

        text_seed.text = "seed: " + WorldManager.world.seed;
        text_time.text = "time: " + WorldManager.world.time;
    }
}
