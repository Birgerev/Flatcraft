public class Torch : Block
{
    public override string[] randomTextures { get; } =
    {
        "block_torch", "block_torch_1", "block_torch_2", "block_torch_3"
    };
    public override float changeTextureTime { get; } = 0.3f;
    public override bool solid { get; set; } = false;
    public override float breakTime { get; } = 0.1f;

    public override int glowLevel { get; } = 15;

    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Wood;
    
    public override void Tick()
    {
        base.Tick();

        CheckIsAttached();
    }

    private void CheckIsAttached()
    {
        //Define relative points we can attach to
        Location[] attachmentLocations = 
        {
            new (0, -1), new (-1, 0), new (1, 0)
        };

        foreach (Location attachmentLocation in attachmentLocations)
        {
            Location worldLoc = location + attachmentLocation;

            //If a valid attachment point is found, no need to keep looking
            if (worldLoc.GetMaterial() != Material.Air)
                return;
        }
        
        //If no valid attachment points are found, self destruct
        Break();
    }
}