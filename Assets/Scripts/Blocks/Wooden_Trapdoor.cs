public class Wooden_Trapdoor : Door
{
    public override string texture { get; set; } = "block_wooden_trapdoor_close";
    public override string open_texture { get; } = "block_wooden_trapdoor_open";
    public override string closed_texture { get; } = "block_wooden_trapdoor_close";
    public override float breakTime { get; } = 3f;
    public override bool isFlammable { get; } = true;
    public override bool climbable { get; } = true;

    public override Tool_Type propperToolType { get; } = Tool_Type.Axe;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Wood;

    public override void Tick()
    {
        var open = data.GetData("open") == "true";

        trigger = open;                             //Custom solution, so that block becomes trigger (and as such climbable), when trapdoor is open
        UpdateColliders();

        base.Tick();
    }
}