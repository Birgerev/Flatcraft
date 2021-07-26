public class Flint_And_Steel : Tool
{
    public override string texture { get; set; } = "item_flint_and_steel";
    public override Tool_Type tool_type { get; } = Tool_Type.FlintAndSteel;
    public override Tool_Level tool_level { get; } = Tool_Level.None;
    public override int maxDurability { get; } = 64;
}