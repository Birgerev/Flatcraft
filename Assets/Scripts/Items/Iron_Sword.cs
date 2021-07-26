public class Iron_Sword : Sword
{
    public override string texture { get; set; } = "item_iron_sword";
    public override Tool_Level tool_level { get; } = Tool_Level.Iron;
    public override int maxDurability { get; } = 250;
    public override float entityDamage { get; } = 6;
}