using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crafting_Table : Block
{
    public static string default_texture = "block_crafting_table";
    public override float breakTime { get; } = 3;

    public override Tool_Type propperToolType { get; } = Tool_Type.Axe;

    public override void Tick()
    {
        base.Tick();
    }

    public override void Interact()
    {
        new CraftingInventory().ToggleOpen();
    }
}
