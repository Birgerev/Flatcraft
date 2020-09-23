using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Log : Block
{
    public override bool playerCollide { get; } = false;
    
    public override float breakTime { get; } = 3f;
    
    public override Tool_Type propperToolType { get; } = Tool_Type.Axe;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Wood;
    
    public override void Initialize()
    {
        base.Initialize();
        
        bool leafTexture = (data.GetData("leaf_texture") == "true");
        if (leafTexture)
        {
            texture = "block_logged_leaves";
            Render();
        }
    }
}
