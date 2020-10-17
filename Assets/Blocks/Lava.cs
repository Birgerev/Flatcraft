public class Lava : Liquid
{
    public static string default_texture = "block_lava";

    public override int max_liquid_level { get; } = 4;
    public override int glowLevel { get; } = 15;
    
    public override void LiquidEncounterFlow(Location relativeLocation)
    {
        Location loc = location + relativeLocation;

        if (relativeLocation.y != 0)        // Lava only effects when flowing down to water, which creates regular stone
        {
            loc.SetMaterial(Material.Stone).Tick();
            
            base.LiquidEncounterFlow(relativeLocation);
        }
    }
}