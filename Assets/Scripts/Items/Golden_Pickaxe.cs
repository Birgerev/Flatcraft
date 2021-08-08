public class Golden_Pickaxe : Pickaxe
{
    public override string texture { get; set; } = "item_golden_pickaxe";
    public override Tool_Level tool_level { get; } = Tool_Level.Gold;
    public override int maxDurability { get; } = 32;
}