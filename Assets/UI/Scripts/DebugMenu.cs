using UnityEngine;
using UnityEngine.UI;

public class DebugMenu : MonoBehaviour
{
    public static bool active;
    public Text text_title;
    public Text text_biome;
    public Text text_dimension;
    public Text text_entityCount;

    public Text text_fps;
    public Text text_seed;
    public Text text_worldTime;
    public Text text_weatherTime;
    public Text text_x;
    public Text text_y;
    public Text text_blockInfo;

    private float deltaTime;

    // Update is called once per frame
    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        UpdateState();

        if (active)
            UpdateText();
    }

    private void UpdateState()
    {
        if (Input.GetKeyDown(KeyCode.F3))
            active = !active;

        GetComponent<CanvasGroup>().alpha = active ? 1 : 0;
        GetComponent<CanvasGroup>().interactable = active;
        GetComponent<CanvasGroup>().blocksRaycasts = active;
    }

    private void UpdateText()
    {
        text_title.text = "Flatcraft " + VersionController.GetVersionName();
        text_fps.text = "fps: " + (int) (1.0f / deltaTime);

        text_entityCount.text = "entity count: " + Entity.EntityCount + ",  living: " + Entity.LivingEntityCount;

        Player player = Player.localEntity;
        text_x.text = "x: " + player.Location.x;
        text_y.text = "y: " + player.Location.y;
        text_dimension.text = "dimension: " + player.Location.dimension;

        Biome biome = new ChunkPosition(player.Location).GetChunk().GetBiome();
        if (biome != null)
            text_biome.text = "biome: " + biome.name;

        //TODO wont sync in multiplayer
        text_seed.text = "seed: " + WorldManager.world.seed;
        text_worldTime.text = "time: " + (int) WorldManager.instance.worldTime + ", (day " +
                              (int) (WorldManager.instance.worldTime / WorldManager.DayLength) + ")";
        text_weatherTime.text = "weather time: " + (int)WorldManager.world.weatherTime;

        Location location = player.GetBlockedMouseLocation();
        BlockState state = location.GetState();

        text_blockInfo.text = "{" + state.data.ToString() + "}   " + "Material." + state.material;
    }
}