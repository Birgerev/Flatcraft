public class Wheat_Crop : Crop
{
    public override string[] crop_textures { get; } =
        {"wheat_crop", "wheat_crop_1", "wheat_crop_2", "wheat_crop_3"};

    public override Material seed { get; } = Material.Wheat_Seeds;
    public override Material result { get; } = Material.Wheat;
}