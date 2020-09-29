public class Iron_Hoe : Tool
{
    public static string default_texture = "item_iron_hoe";
    public override Tool_Type tool_type { get; } = Tool_Type.Hoe;
    public override Tool_Level tool_level { get; } = Tool_Level.Iron;
    public override int maxDurabulity { get; } = 250;
}