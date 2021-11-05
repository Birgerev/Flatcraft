public class Wooden_Door_Top : Wooden_Door_Block
{
    public override string texture { get; set; } = "block_wooden_door_top_close";
    public override bool rotateX { get; } = true;

    public override string open_texture { get; } = "block_wooden_door_top_open";
    public override string closed_texture { get; } = "block_wooden_door_top_close";
}