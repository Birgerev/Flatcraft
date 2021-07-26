public class Diamond_Sword : Sword
{
    public override string texture { get; set; } = "item_diamond_sword";
    public override Tool_Level tool_level { get; } = Tool_Level.Diamond;
    public override int maxDurabulity { get; } = 1561;
    public override float entityDamage { get; } = 7;
}