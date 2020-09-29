public class Diamond_Hoe : Tool
{
    public static string default_texture = "item_diamond_hoe";
    public override Tool_Type tool_type { get; } = Tool_Type.Hoe;
    public override Tool_Level tool_level { get; } = Tool_Level.Diamond;
    public override int maxDurabulity { get; } = 1561;
}