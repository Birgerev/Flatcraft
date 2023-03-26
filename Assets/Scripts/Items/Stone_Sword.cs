public class Stone_Sword : Sword
{
    public override Tool_Level tool_level { get; } = Tool_Level.Stone;
    public override int maxDurability { get; } = 131;
    public override float entityDamage { get; } = 5;
}