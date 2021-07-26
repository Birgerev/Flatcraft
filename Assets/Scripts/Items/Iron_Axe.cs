public class Iron_Axe : Axe
{
    public override string texture { get; set; } = "item_iron_axe";
    public override Tool_Level tool_level { get; } = Tool_Level.Iron;
    public override int maxDurabulity { get; } = 250;
}