public class Golden_Hoe : Hoe
{
    public override string texture { get; set; } = "item_golden_hoe";
    public override Tool_Level tool_level { get; } = Tool_Level.Gold;
    public override int maxDurabulity { get; } = 32;
}