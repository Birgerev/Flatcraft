public class Wooden_Sword : Sword
{
    public override string texture { get; set; } = "item_wooden_sword";
    public override Tool_Level tool_level { get; } = Tool_Level.Wooden;
    public override int maxDurabulity { get; } = 59;
    public override float entityDamage { get; } = 4;
}