using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.Burst;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

[BurstCompile]
public class Block : MonoBehaviour
{
    //Block Properties
    public virtual bool IsSolid { get; set; } = true;
    public virtual bool IsFlammable { get; } = false;
    public virtual bool IsClimbable { get; } = false;
    public virtual bool RequiresGround { get; } = false;
    
    public virtual float AverageRandomTickDuration { get; } = 0;
    public virtual bool RotateX { get; } = false;
    public virtual bool RotateY { get; } = false;
    public virtual LightValues LightSourceValues { get; } = new LightValues(0);
    
    //Block Rendering
    public virtual string[] RandomTextures { get; } = { };
    public virtual float ChangeTextureTime { get; } = 0;

    //Block Breaking
    public virtual Tool_Type ProperToolType { get; } = Tool_Type.None;
    public virtual Tool_Level ProperToolLevel { get; } = Tool_Level.None;
    public virtual BlockSoundType BlockSoundType { get; } = BlockSoundType.Stone;
    public virtual float BreakTime { get; } = 0.75f;
    
    //Runtime Values
    public float blockDamage;
    public Location location;
    private float _timeOfLastHit;
    
    #region Ticking
    public virtual void Initialize()
    {
        //If this is a light source, update light
        if (LightSourceValues.lightLevel > 0)
            LightSource.Create(transform).UpdateLightValues(LightSourceValues, true);
        
        if (ChangeTextureTime > 0)
            InvokeRepeating(nameof(UpdateRender), 0, ChangeTextureTime);

        gameObject.layer = LayerMask.NameToLayer(IsSolid ? "Block" : "NoCollision");
        
        UpdateRender();
    }
    
    public virtual void Tick()
    {
        gameObject.layer = LayerMask.NameToLayer(IsSolid ? "Block" : "NoCollision");
        
        UpdateRender();
    }
    
    public virtual void RandomTick() { }

    public virtual void ServerInitialize() { }

    public virtual void GeneratingTick() { }

    public virtual void BuildTick()
    {
        if (new ChunkPosition(location).GetChunk().isLoaded) //Block place sound
            Sound.Play(location, "block/" + BlockSoundType.ToString().ToLower() + "/break", SoundType.Block, 0.5f
                , 1.5f);

        if ((RotateX || RotateY) && !(GetData().HasTag("rotated_x") || GetData().HasTag("rotated_y")))
            UpdateRotation();
    }
    #endregion

    #region Interaction
    public virtual void Interact(PlayerInstance player)
    {
    }

    public virtual async void Hit(PlayerInstance player, float time, Tool_Type tool_type = Tool_Type.None, Tool_Level tool_level = Tool_Level.None)
    {
        bool properToolStats = false;

        if (tool_level != Tool_Level.None && tool_type == ProperToolType && tool_level >= ProperToolLevel)
            time *= 2 + (float) tool_level * 2f;
        if (ProperToolLevel == Tool_Level.None ||
            tool_type == ProperToolType && tool_level >= ProperToolLevel)
            properToolStats = true;

        blockDamage += time;

        Sound.Play(location, "block/" + BlockSoundType.ToString().ToLower() + "/hit", SoundType.Block, 0.8f, 1.2f);

        if (!BreakIndicator.breakIndicators.ContainsKey(location))
            BreakIndicator.Spawn(location);

        if (blockDamage >= BreakTime)
        {
            if (properToolStats)
                Break();
            else
                Break(false);

            player.playerEntity.GetComponent<Player>().DoToolDurability();

            return;
        }

        _timeOfLastHit = Time.time;
        
        await Task.Delay(1 * 1000);

        if(Time.time - _timeOfLastHit >= 1)
            blockDamage = 0;
    }
    
    public virtual void Break(bool drop = true)
    {
        if (drop)
        {
            ItemStack[] drops = GetDrops();
            if (drops != null)//TODO null error
                foreach(ItemStack item in GetDrops())
                    item.Drop(location);
        }

        Sound.Play(location, "block/" + BlockSoundType.ToString().ToLower() + "/break", SoundType.Block, 0.5f, 1.5f);

        //Play block break effect to all clients
        new ChunkPosition(location).GetChunk().BlockBreakParticleEffect(location, GetColorsInTexture());
        
        location.SetMaterial(Material.Air).Tick();
    }
    #endregion
    
    protected void UpdateRender()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        
        //Update sprite
        renderer.sprite = GetSprite();
        
        //Render rotation
        BlockData data = GetData();
        renderer.flipX = data.GetTag("rotated_x") == "true";
        renderer.flipY = data.GetTag("rotated_y") == "true";
    }
    
    
    private void UpdateRotation()
    {
        Player closestPlayer = (Player) Entity.ClosestEntityOfType(location, typeof(Player));
        if (closestPlayer == null) return;
        
        bool rotated_y = RotateY && closestPlayer.transform.position.y + 1 < location.y;
        bool rotated_x = RotateX && closestPlayer.transform.position.x < location.x;

        BlockData newData = GetData();
        newData.SetTag("rotated_x", rotated_x ? "true" : "false");
        newData.SetTag("rotated_y", rotated_y ? "true" : "false");
        SetData(newData);
    }
    
    protected virtual ItemStack[] GetDrops()
    {
        return new[] { new ItemStack(GetMaterial()) };
    }

    public Material GetMaterial()
    {
        return (Material) Enum.Parse(typeof(Material), GetType().Name);
    }
    
    public BlockData GetData()
    {
        return location.GetData();
    }

    protected Location SetData(BlockData data)
    {
        return location.SetData(data);
    }

    private Sprite GetSprite()
    {
        return Resources.Load<Sprite>("Sprites/block/" + GetTextureName());
    }
    
    protected virtual string GetTextureName()
    {
        if (RandomTextures.Length > 0) 
            return GetRandomTexture();
        
        return GetMaterial().ToString();
    }

    #region Bad Code
    public Color[] GetColorsInTexture()
    {
        //TODO move to particle
        //TODO also remove ItemStack.Colors
        Texture2D texture = GetSprite().texture;

        return texture.GetPixels();
    }
    
    private string GetRandomTexture()
    {
        //Default get a random alternative texture based on location
        int textureIndex = new Random(SeedGenerator.SeedByWorldLocation(location)).Next(0, RandomTextures.Length);

        //Textures that change over time
        if (ChangeTextureTime > 0)
        {
            float totalTimePerTextureLoop = ChangeTextureTime * RandomTextures.Length;
            textureIndex = (int) (Time.time % totalTimePerTextureLoop / ChangeTextureTime);
        }

        return RandomTextures[textureIndex];
    }
    #endregion
}