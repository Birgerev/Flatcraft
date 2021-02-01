public class Stone_Hoe : Tool
{
    public override string texture { get; set; } = "item_stone_hoe";
    public override Tool_Type tool_type { get; } = Tool_Type.Hoe;
    public override Tool_Level tool_level { get; } = Tool_Level.Stone;
    public override int maxDurabulity { get; } = 131;
}