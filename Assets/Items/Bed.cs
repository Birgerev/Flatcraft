public class Bed : PlaceableItem
{
    public static string default_texture = "item_bed";
    public override Material blockMaterial { get; } = Material.Bed_Bottom;
}