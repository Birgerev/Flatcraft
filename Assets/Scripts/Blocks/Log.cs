public class Log : Block
{
    public override bool solid { get; set; } = false;

    public override float breakTime { get; } = 3f;
    public override bool isFlammable { get; } = true;

    public override Tool_Type properToolType { get; } = Tool_Type.Axe;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Wood;

    public override string GetTexture()
    {
        bool leafTexture = GetData().GetTag("leaf_texture") == "true";
        if (leafTexture)
        {
            return "block_logged_leaves";
        }

        return base.GetTexture();
    }
}