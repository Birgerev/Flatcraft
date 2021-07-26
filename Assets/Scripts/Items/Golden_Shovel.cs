public class Golden_Shovel : Shovel
{
    public override string texture { get; set; } = "item_golden_shovel";
    public override Tool_Level tool_level { get; } = Tool_Level.Gold;
    public override int maxDurabulity { get; } = 32;
}