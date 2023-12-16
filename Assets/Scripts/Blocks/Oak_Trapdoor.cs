public class Oak_Trapdoor : Door
{
    public override string open_texture { get; } = "oak_trapdoor_open";
    public override string closed_texture { get; } = "oak_trapdoor";
    public override float BreakTime { get; } = 3f;
    public override bool IsFlammable { get; } = true;
    public override bool Climbable { get; } = true;

    public override Tool_Type ProperToolType { get; } = Tool_Type.Axe;
    public override BlockSoundType BlockSoundType { get; } = BlockSoundType.Wood;

    public override void Initialize()
    {
        bool open = GetData().GetTag("open") == "true";

        Trigger = open; //Custom solution, so that block becomes trigger (and as such climbable), when trapdoor is open
        UpdateColliders();

        base.Initialize();
    }
}