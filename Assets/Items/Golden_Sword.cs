public class Golden_Sword : Tool
{
    public static string default_texture = "item_golden_sword";
    public override Tool_Type tool_type { get; } = Tool_Type.Sword;
    public override Tool_Level tool_level { get; } = Tool_Level.Gold;
    public override int maxDurabulity { get; } = 32;
}