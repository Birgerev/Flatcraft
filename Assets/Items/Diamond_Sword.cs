public class Diamond_Sword : Tool
{
    public static string default_texture = "item_diamond_sword";
    public override Tool_Type tool_type { get; } = Tool_Type.Sword;
    public override Tool_Level tool_level { get; } = Tool_Level.Diamond;
    public override int maxDurabulity { get; } = 1561;
    public override float entityDamage { get; } = 7;
}