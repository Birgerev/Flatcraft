using System;
using UnityEngine;
using System.Collections;

public class Chest : InventoryContainer
{
    public override string texture { get; set; } = closed_texture;
    public static string closed_texture = "block_chest_closed";
    public static string open_texture = "block_chest_open";
    public override bool solid { get; set; } = false;
    public override bool rotate_x { get; } = true;
    
    public override float breakTime { get; } = 6;

    public override Tool_Type propperToolType { get; } = Tool_Type.Axe;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Wood;

    public override Inventory NewInventory()
    {
        return Inventory.Create("Inventory", 27, "Chest");
    }
    
    public override void Interact(PlayerInstance player)
    {
        base.Interact(player);
        
        StartCoroutine(ChestEffects());
    }

    IEnumerator ChestEffects()
    {
        texture = open_texture;
        Render();
        Sound.Play(location, "random/door/door_open", SoundType.Blocks, 0.8f, 1.3f);
        yield return new WaitForSeconds(0.5f);
        while (GetInventory().open)
            yield return new WaitForSeconds(0.2f);

        Sound.Play(location, "random/door/door_close", SoundType.Blocks, 0.8f, 1.3f);

        yield return new WaitForSeconds(0.1f);
        texture = closed_texture;
        Render();
    }
}