public class Wheat_Seeds : PlaceableItem
{
    public override string texture { get; set; } = "item_wheat_seeds";
    public override Material blockMaterial { get; } = Material.Wheat_Crop;
}