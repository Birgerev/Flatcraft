public class Stone_Sword : Tool
{
    public static string default_texture = "item_stone_sword";
    public override Tool_Type tool_type { get; } = Tool_Type.Sword;
    public override Tool_Level tool_level { get; } = Tool_Level.Stone;
    public override int maxDurabulity { get; } = 131;
}