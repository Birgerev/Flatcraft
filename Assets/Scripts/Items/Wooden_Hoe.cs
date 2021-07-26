public class Wooden_Hoe : Hoe
{
    public override string texture { get; set; } = "item_wooden_hoe";
    public override Tool_Level tool_level { get; } = Tool_Level.Wooden;
    public override int maxDurability { get; } = 59;
}