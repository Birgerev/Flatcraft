using System;

public class Crop : Block
{
    public override bool IsSolid { get; set; } = false;

    public override float BreakTime { get; } = 0.01f;
    public override bool IsFlammable { get; } = true;
    public override float AverageRandomTickDuration { get; } = 100;

    public override BlockSoundType BlockSoundType { get; } = BlockSoundType.Grass;

    public virtual string[] crop_textures { get; } = { };
    public virtual Material seed { get; } = Material.Air;
    public virtual Material result { get; } = Material.Air;

    public override void BuildTick()
    {
        if (!GetData().HasTag("crop_stage"))
            SetData(GetData().SetTag("crop_stage", "0"));
    }

    protected override string GetTextureName()
    {
        return crop_textures[GetStage()];
    }

    public override void RandomTick()
    {
        Grow();
        Tick();

        base.RandomTick();
    }
    
    public override bool CanExistAt(Location loc)
    {
        Material belowMat = (loc + new Location(0, -1)).GetMaterial();

        if (belowMat != Material.Farmland_Wet && belowMat != Material.Farmland_Dry) return false;

        return base.CanExistAt(loc);
    }
    
    public virtual void Grow()
    {
        if (GetStage() >= GetAmountOfStages() - 1)
            return;

        location.SetData(GetData().SetTag("crop_stage", (GetStage() + 1).ToString())).Tick();
    }

    public int GetAmountOfStages()
    {
        return crop_textures.Length;
    }

    public int GetStage()
    {
        string stageTag = GetData().GetTag("crop_stage");
        int.TryParse(stageTag, out var stage);
        return stage;
    }

    protected override ItemStack[] GetDrops()
    {
        if (GetStage() == GetAmountOfStages() - 1)
        {
            return new[] { 
                new ItemStack(seed, new Random().Next(1, 3 + 1)), 
                new ItemStack(result)};
        }

        return new[] { new ItemStack(seed)};
    }
}