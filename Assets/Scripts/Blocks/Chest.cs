using System.Collections;
using UnityEngine;

public class Chest : InventoryContainer
{
    public static string closed_texture = "block_chest_closed";
    public static string open_texture = "block_chest_open";
    public override string texture { get; set; } = closed_texture;
    public override bool solid { get; set; } = false;
    public override bool rotate_x { get; } = true;

    public override float breakTime { get; } = 6;

    public override Tool_Type propperToolType { get; } = Tool_Type.Axe;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Wood;

    public override void Initialize()
    {
        base.Initialize();

        StartCoroutine(AwaitChestOpenCloseTextureChange());
    }

    public override void ServerInitialize()
    {
        base.ServerInitialize();

        StartCoroutine(AwaitChestOpenCloseSound());
    }

    public override Inventory NewInventory()
    {
        return Inventory.Create("Inventory", 27, "Chest");
    }

    private bool IsOpen()
    {
        return GetInventory().open;
    }

    private IEnumerator AwaitChestOpenCloseTextureChange()
    {
        bool lastCheckOpen = IsOpen();
        while (true)
        {
            if (lastCheckOpen != IsOpen())
            {
                yield return new WaitForSeconds(0.1f);
                texture = IsOpen() ? open_texture : closed_texture;
                Render();

                lastCheckOpen = IsOpen();
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator AwaitChestOpenCloseSound()
    {
        bool lastCheckOpen = IsOpen();
        while (true)
        {
            if (lastCheckOpen != IsOpen())
            {
                if (IsOpen())
                    Sound.Play(location, "random/door/door_open", SoundType.Blocks, 0.8f, 1.3f);
                else
                    Sound.Play(location, "random/door/door_close", SoundType.Blocks, 0.8f, 1.3f);

                lastCheckOpen = IsOpen();
            }

            yield return new WaitForSeconds(0.2f);
        }
    }
}