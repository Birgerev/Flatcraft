public class Wooden_Door_Top : Wooden_Door
{
    public static string default_texture = "block_wooden_door_top_close";
    public override bool rotate_x { get; } = true;

    public override string open_texture { get; } = "block_wooden_door_top_open";
    public override string closed_texture { get; } = "block_wooden_door_top_close";
}