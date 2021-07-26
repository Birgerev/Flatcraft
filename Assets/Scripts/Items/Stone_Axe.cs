public class Stone_Axe : Axe
{
    public override string texture { get; set; } = "item_stone_axe";
    public override Tool_Level tool_level { get; } = Tool_Level.Stone;
    public override int maxDurabulity { get; } = 131;
}