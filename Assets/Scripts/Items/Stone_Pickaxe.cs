public class Stone_Pickaxe : Pickaxe
{
    public override string texture { get; set; } = "item_stone_pickaxe";
    public override Tool_Level tool_level { get; } = Tool_Level.Stone;
    public override int maxDurability { get; } = 131;
}