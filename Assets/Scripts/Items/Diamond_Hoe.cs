public class Diamond_Hoe : Hoe
{
    public override string texture { get; set; } = "item_diamond_hoe";
    public override Tool_Level tool_level { get; } = Tool_Level.Diamond;
    public override int maxDurability { get; } = 1561;
}