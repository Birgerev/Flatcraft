public class Oak_Door_Bottom : Oak_Door_Block
{
    public override bool rotateX { get; } = true;

    public override string open_texture { get; } = "block_wooden_door_bottom_open";
    public override string closed_texture { get; } = "block_wooden_door_bottom";
}