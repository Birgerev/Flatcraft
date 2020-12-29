public class Stone_Sword : Tool
{
    public override string texture { get; set; } = "item_stone_sword";
    public override Tool_Type tool_type { get; } = Tool_Type.Sword;
    public override Tool_Level tool_level { get; } = Tool_Level.Stone;
    public override int maxDurabulity { get; } = 131;
    public override float entityDamage { get; } = 5;
}