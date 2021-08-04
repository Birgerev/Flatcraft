public class Golden_Axe : Axe
{
    public override string texture { get; set; } = "item_golden_axe";
    public override Tool_Level tool_level { get; } = Tool_Level.Gold;
    public override int maxDurability { get; } = 32;
}