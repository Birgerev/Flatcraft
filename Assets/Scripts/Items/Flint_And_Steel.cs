public class Flint_And_Steel : Item
{
    public override int maxDurability { get; } = 64;
    
    protected override void InteractRight(PlayerInstance player, Location loc, bool firstFrameDown)
    {
        if (loc.GetMaterial() == Material.Air)
        {
            loc.SetMaterial(Material.Fire).Tick();
            player.playerEntity.GetComponent<Player>().DoToolDurability();
            Sound.Play(loc, "random/flint_and_steel/click", SoundType.Entities, 0.8f, 1.2f);
        }
        if (loc.GetMaterial() == Material.TNT)
        {
            ((TNT) loc.GetBlock()).Prime();
            player.playerEntity.GetComponent<Player>().DoToolDurability();
            Sound.Play(loc, "random/flint_and_steel/click", SoundType.Entities, 0.8f, 1.2f);
        }

        base.InteractRight(player, loc, firstFrameDown);
    }
}