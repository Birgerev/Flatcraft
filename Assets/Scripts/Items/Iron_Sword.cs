public class Iron_Sword : Sword
{
    public override Tool_Level tool_level { get; } = Tool_Level.Iron;
    public override int maxDurability { get; } = 250;
    public override float entityDamage { get; } = 6;
}