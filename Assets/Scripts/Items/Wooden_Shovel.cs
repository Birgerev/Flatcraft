public class Wooden_Shovel : Shovel
{
    public override string texture { get; set; } = "item_wooden_shovel";
    public override Tool_Level tool_level { get; } = Tool_Level.Wooden;
    public override int maxDurability { get; } = 59;
}