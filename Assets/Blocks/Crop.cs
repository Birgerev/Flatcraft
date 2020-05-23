using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crop : Block
{
    public override bool playerCollide { get; } = false;

    public override float breakTime { get; } = 0.05f;
    public override bool autoTick { get; } = true;
    public override bool autosave { get; } = true;
    public override bool requiresGround { get; } = true;
    
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Grass;
    
    public virtual string[] crop_textures { get; } = {};
    public virtual Material seed { get; } = Material.Air;
    public virtual Material result { get; } = Material.Air;
    
    
    public override void Tick(bool spreadTick)
    {
        if (getRandomChance() < 0.5f)
            Grow();
        
        texture = crop_textures[GetStage()];
        Render();
        
        base.Tick(spreadTick);
    }

    public void CheckFarmland()
    {
        Block blockBeneath = Chunk.getBlock(location - new Location(0, 1));

        if (blockBeneath == null || (blockBeneath.GetMaterial() != Material.Farmland_Wet && blockBeneath.GetMaterial() != Material.Farmland_Dry))
        {
            Break();
        }
    }
    
    public void Grow()
    {
        if (GetStage() >= GetAmountOfStages() - 1)
            return;
        
        data["crop_stage"] = (GetStage() + 1).ToString();
    }

    public int GetAmountOfStages()
    {
        return crop_textures.Length;
    }
    
    public int GetStage()
    {
        if (!data.ContainsKey("crop_stage"))
            data["crop_stage"] = "0";
        
        return int.Parse(data["crop_stage"]);
    }
    
    public override void Drop()
    {
        new ItemStack(seed, new System.Random().Next(0, 3)).Drop(location);
        
        if(GetStage() == GetAmountOfStages() - 1)
            new ItemStack(result, 1).Drop(location);
    }
}
