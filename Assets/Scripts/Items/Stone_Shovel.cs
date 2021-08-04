public class Stone_Shovel : Shovel
{
    public override string texture { get; set; } = "item_stone_shovel";
    public override Tool_Level tool_level { get; } = Tool_Level.Stone;
    public override int maxDurability { get; } = 131;
}