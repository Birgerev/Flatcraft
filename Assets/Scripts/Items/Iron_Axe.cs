public class Iron_Axe : Tool
{
    public override string texture { get; set; } = "item_iron_axe";
    public override Tool_Type tool_type { get; } = Tool_Type.Axe;
    public override Tool_Level tool_level { get; } = Tool_Level.Iron;
    public override int maxDurabulity { get; } = 250;
}