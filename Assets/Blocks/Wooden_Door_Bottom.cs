using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wooden_Door_Bottom : Door
{
    public override bool rotate_x { get; } = true;

    public override string open_texture { get; } = "block_wooden_door_bottom_open";
    public override string closed_texture { get; } = "block_wooden_door_bottom_close";

    public static string default_texture = "block_wooden_door_bottom_close";
    public override float breakTime { get; } = 3f;

    public override Tool_Type propperToolType { get; } = Tool_Type.Axe;

    public override void FirstTick()
    {
        Chunk.setBlock(getPosition() + new Vector2Int(0, 1), Material.Wooden_Door_Top);

        base.FirstTick();
    }

    public override void Break()
    {
        Block block = Chunk.getBlock(getPosition() + new Vector2Int(0, 1));
        if (block != null && block.GetMaterial() == Material.Wooden_Door_Top)
        {
            Chunk.setBlock(getPosition() + new Vector2Int(0, 1), Material.Air);
        }

        base.Break();
    }

    public override void Interact()
    {
        Door block = (Door)Chunk.getBlock(getPosition() + new Vector2Int(0, 1));

        block.ToggleOpen();

        base.Interact();
    }
}
