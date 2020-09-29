public class Diamond_Shovel : Tool
{
    public static string default_texture = "item_diamond_shovel";
    public override Tool_Type tool_type { get; } = Tool_Type.Shovel;
    public override Tool_Level tool_level { get; } = Tool_Level.Diamond;
    public override int maxDurabulity { get; } = 1561;
}