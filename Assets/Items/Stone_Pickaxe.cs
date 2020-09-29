public class Stone_Pickaxe : Tool
{
    public static string default_texture = "item_stone_pickaxe";
    public override Tool_Type tool_type { get; } = Tool_Type.Pickaxe;
    public override Tool_Level tool_level { get; } = Tool_Level.Stone;
    public override int maxDurabulity { get; } = 131;
}