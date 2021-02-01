using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class DebugMenu : MonoBehaviour
{
    public static bool active;

    private float deltaTime;
    public Text text_title;
    public Text text_biome;
    public Text text_dimension;
    public Text text_entityCount;

    public Text text_fps;
    public Text text_seed;
    public Text text_time;
    public Text text_x;
    public Text text_y;
    public Text text_blockInfo;

    // Update is called once per frame
    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        updateState();

        if (active)
            updateText();
    }

    private void updateState()
    {
        if (Input.GetKeyDown(KeyCode.F3))
            active = !active;

        GetComponent<CanvasGroup>().alpha = active ? 1 : 0;
        GetComponent<CanvasGroup>().interactable = active;
        GetComponent<CanvasGroup>().blocksRaycasts = active;
    }

    private void updateText()
    {
        text_title.text = "Flatcraft " + VersionController.GetVersionName();
        text_fps.text = "fps: " + (int) (1.0f / deltaTime);

        text_entityCount.text = "entity count: " + Entity.EntityCount + ",  living: " + Entity.LivingEntityCount;

        var player = Player.localInstance;
        text_x.text = "x: " + player.transform.position.x;
        text_y.text = "y: " + player.transform.position.y;
        text_dimension.text = "dimension: " + player.Location.dimension;
        var biome = new ChunkPosition(player.Location).GetChunk().GetBiome();
        if (biome != null)
            text_biome.text = "biome: " + biome.name;

        text_seed.text = "seed: " + WorldManager.world.seed;
        text_time.text = "time: " + (int) WorldManager.world.time + ", (day " +
                         (int) (WorldManager.world.time / WorldManager.dayLength) + ")";

        var block = player.GetMouseBlock();
        var material = Material.Air;
        var data = "{}";
        if(block != null)
        {
            material = block.GetMaterial();
            data = block.data.GetSaveString();
        }
        text_blockInfo.text = "Material." + material.ToString() + " " + data;
    }
}