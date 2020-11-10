public class Wooden_Sword : Tool
{
    public static string default_texture = "item_wooden_sword";
    public override Tool_Type tool_type { get; } = Tool_Type.Sword;
    public override Tool_Level tool_level { get; } = Tool_Level.Wooden;
    public override int maxDurabulity { get; } = 59;
    public override float entityDamage { get; } = 4;
}