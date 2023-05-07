public class Oak_Trapdoor : Door
{
    public override string open_texture { get; } = "oak_trapdoor_open";
    public override string closed_texture { get; } = "oak_trapdoor";
    public override float breakTime { get; } = 3f;
    public override bool isFlammable { get; } = true;
    public override bool climbable { get; } = true;

    public override Tool_Type properToolType { get; } = Tool_Type.Axe;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Wood;

    public override void Initialize()
    {
        bool open = GetData().GetTag("open") == "true";

        trigger = open; //Custom solution, so that block becomes trigger (and as such climbable), when trapdoor is open
        UpdateColliders();

        base.Initialize();
    }
}