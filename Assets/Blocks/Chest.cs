using System;
using UnityEngine;
using System.Collections;

public class Chest : InventoryContainer
{
    public static string default_texture = "block_chest_closed";
    public static string open_texture = "block_chest_open";
    public override bool solid { get; set; } = false;
    public override bool rotate_x { get; } = true;
    
    public override float breakTime { get; } = 6;

    public override Tool_Type propperToolType { get; } = Tool_Type.Axe;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Wood;

    public override Type inventoryType { get; } = typeof(Inventory);

    public override void Tick()
    {
        if (inventory == null)
        {
            base.Tick();
            return;
        }

        inventory.name = "Chest";
        base.Tick();
    }

    private Inventory getInventory()
    {
        return inventory;
    }
    public override void Interact()
    {
        StartCoroutine(awaitInventoryClosed());

        base.Interact();
    }

    IEnumerator awaitInventoryClosed()
    {
        texture = open_texture;
        Render();
        Sound.Play(location, "random/door/door_open", SoundType.Blocks, 0.8f, 1.3f);
        yield return new WaitForSeconds(0.5f);
        while (getInventory().open)
            yield return new WaitForSeconds(0.2f);

        Sound.Play(location, "random/door/door_close", SoundType.Blocks, 0.8f, 1.3f);

        yield return new WaitForSeconds(0.1f);
        texture = default_texture;
        Render();
    }
}