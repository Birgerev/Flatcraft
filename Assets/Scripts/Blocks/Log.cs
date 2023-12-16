public class Log : Block
{
    public override bool solid { get; set; } = false;

    public override float breakTime { get; } = 3f;
    public override bool isFlammable { get; } = true;

    public override Tool_Type properToolType { get; } = Tool_Type.Axe;
    public override BlockSoundType blockSoundType { get; } = BlockSoundType.Wood;

    public override string GetTexture()
    {
        bool leafTexture = GetData().GetTag("leaf_texture") == "true";
        if (leafTexture)
        {
            return "logged_leaves";
        }

        return base.GetTexture();
    }
}