public class Iron_Shovel : Shovel
{
    public override string texture { get; set; } = "item_iron_shovel";
    public override Tool_Level tool_level { get; } = Tool_Level.Iron;
    public override int maxDurability { get; } = 250;
}