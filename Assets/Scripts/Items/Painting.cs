public class Painting : PlaceableEntityItem
{
    public override string entityType { get; } = "PaintingEntity";
    public override string texture { get; set; } = "item_painting";
}