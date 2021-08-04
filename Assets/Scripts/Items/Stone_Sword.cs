public class Stone_Sword : Sword
{
    public override string texture { get; set; } = "item_stone_sword";
    public override Tool_Level tool_level { get; } = Tool_Level.Stone;
    public override int maxDurability { get; } = 131;
    public override float entityDamage { get; } = 5;
}