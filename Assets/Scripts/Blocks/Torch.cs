using UnityEngine;

public class Torch : Block
{
    public override string[] RandomTextures { get; } =
    {
        "torch", "torch_1", "torch_2", "torch_3"
    };
    public override float ChangeTextureTime { get; } = 0.3f;
    public override bool IsSolid { get; set; } = false;
    public override float BreakTime { get; } = 0.1f;

    public override LightValues LightSourceValues { get; } = new LightValues(15, new Color(1, .6f, .4f), true);

    public override BlockSoundType BlockSoundType { get; } = BlockSoundType.Wood;
    
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