public class Iron_Sword : Tool
{
    public override string texture { get; set; } = "item_iron_sword";
    public override Tool_Type tool_type { get; } = Tool_Type.Sword;
    public override Tool_Level tool_level { get; } = Tool_Level.Iron;
    public override int maxDurabulity { get; } = 250;
    public override float entityDamage { get; } = 6;
}