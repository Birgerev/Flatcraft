using System.Collections;
using UnityEngine;

public class Chest : InventoryContainer
{
    public static string closed_texture = "chest";
    public static string open_texture = "chest_open";
    public override bool Solid { get; set; } = false;
    public override bool RotateX { get; } = true;

    public override float BreakTime { get; } = 6;

    public override Tool_Type ProperToolType { get; } = Tool_Type.Axe;
    public override BlockSoundType BlockSoundType { get; } = BlockSoundType.Wood;

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
        if (!GetData().HasTag("inventoryId"))
            return false;
        
        return GetInventory().open;
    }

    private IEnumerator AwaitChestOpenCloseTextureChange()
    {
        //Wait for chest values to sync from server
        yield return new WaitForSeconds(1f);
        
        bool lastCheckOpen = IsOpen();
        while (true)
        {
            if (lastCheckOpen != IsOpen())
            {
                yield return new WaitForSeconds(0.1f);
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
                    Sound.Play(location, "random/door/door_open", SoundType.Block, 0.8f, 1.3f);
                else
                    Sound.Play(location, "random/door/door_close", SoundType.Block, 0.8f, 1.3f);

                lastCheckOpen = IsOpen();
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    protected override string GetTexture()
    {
        return IsOpen() ? open_texture : closed_texture;
    }
}