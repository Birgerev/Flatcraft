public class Bed : PlaceableItem
{
    public override string texture { get; set; } = "item_bed";
    public override Material blockMaterial { get; } = Material.Bed_Bottom;
}