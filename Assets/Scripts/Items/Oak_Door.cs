
public class Oak_Door : PlaceableItem
{
    public override string texture { get; set; } = "item_wooden_door";
    public override Material blockMaterial { get; } = Material.Oak_Door_Bottom;
}