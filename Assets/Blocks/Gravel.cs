﻿public class Gravel : Block
{
    public static string default_texture = "block_gravel";
    public override float breakTime { get; } = 0.75f;

    public override Tool_Type propperToolType { get; } = Tool_Type.Shovel;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Gravel;

    public override void Tick()
    {
        if (age > 0 && (location + new Location(0, -1)).GetMaterial() == Material.Air)
        {
            var fs = (FallingSand) Entity.Spawn("FallingSand");
            fs.transform.position = location.GetPosition();
            fs.material = GetMaterial();

            location.SetMaterial(Material.Air).Tick();
        }

        base.Tick();
    }
}