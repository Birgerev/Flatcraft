public class Lava : Liquid
{
    public override string texture { get; set; } = "block_lava";

    public override int max_liquid_level { get; } = 4;
    public override int glowLevel { get; } = 15;
    public override float averageRandomTickDuration { get; } = 60;

    public override void LiquidEncounterFlow(Location relativeLocation)
    {
        Location loc = location + relativeLocation;

        if (relativeLocation.y != 0)        // Lava only effects when flowing down to water, which creates regular stone
        {
            LiquidEncounterEffect(loc);
            loc.SetMaterial(Material.Stone).Tick();
            
            base.LiquidEncounterFlow(relativeLocation);
        }
    }

    public override void RandomTick()
    {
        var random = new System.Random();
        var spreadLocation = new Location();

        int attempts = 0;
        while (spreadLocation.Equals(new Location()))
        {
            var x = random.Next(-1, 1 + 1);    //+1 because upper bound is exclusive
            var y = random.Next(-2, 2 + 1);

            if ((location + new Location(x, y)).GetMaterial() == Material.Air &&
                (location + new Location(x, y - 1)).GetMaterial() != Material.Air &&
                (location + new Location(x, y - 1)).GetBlock().isFlammable)
                (location + new Location(x, y)).SetMaterial(Material.Fire);

            if (attempts > 20)
                return;
            attempts++;
        }

        spreadLocation.SetMaterial(Material.Fire).Tick();

        base.RandomTick();
    }
}