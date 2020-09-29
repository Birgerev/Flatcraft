public class Wheat_Seeds : PlaceableItem
{
    public static string default_texture = "item_wheat_seeds";
    public override Material blockMaterial { get; } = Material.Wheat_Crop;
}