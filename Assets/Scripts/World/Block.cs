using System;
using System.Collections;
using Unity.Burst;
using UnityEngine;
using Random = System.Random;

[BurstCompile]
public class Block : MonoBehaviour
{
    public virtual string[] randomTextures { get; } = { };
    public virtual float changeTextureTime { get; } = 0;

    public virtual bool solid { get; set; } = true;
    public virtual bool isFlammable { get; } = false;
    public virtual bool trigger { get; set; } = false;
    public virtual bool climbable { get; } = false;
    public virtual bool requiresGround { get; } = false;
    public virtual float averageRandomTickDuration { get; } = 0;
    public virtual float breakTime { get; } = 0.75f;
    public virtual bool rotateX { get; } = false;
    public virtual bool rotateY { get; } = false;

    public virtual Tool_Type properToolType { get; } = Tool_Type.None;
    public virtual Tool_Level properToolLevel { get; } = Tool_Level.None;

    public virtual BlockSoundType blockSoundType { get; } = BlockSoundType.Stone;

    public virtual LightValues lightSourceValues { get; } = new LightValues(0);
    
    public float blockHealth;
    public Location location;
    private float timeOfLastHit;

    public void OnDestroy()
    {
        Chunk chunk = new ChunkPosition(location).GetChunk();

        if (chunk != null && chunk.randomTickBlocks.Contains(this))
            chunk.randomTickBlocks.Remove(this);
    }

    public virtual void OnTriggerExit2D(Collider2D col)
    {
        if (climbable)
            if (col.GetComponent<Entity>() != null)
                col.GetComponent<Entity>().isOnClimbable = false;
    }

    public virtual void OnTriggerStay2D(Collider2D col)
    {
        if (climbable)
            if (col.GetComponent<Entity>() != null)
                col.GetComponent<Entity>().isOnClimbable = true;
    }

    public virtual void Initialize()
    {
        //Cache position for use in multithreading
        location = Location.LocationByPosition(transform.position);

        blockHealth = breakTime;

        RenderRotate();
        UpdateColliders();

        if (lightSourceValues.lightLevel > 0)
        {
            LightSource source = LightSource.Create(transform);

            source.UpdateLightValues(lightSourceValues, true);
        }

        if (changeTextureTime != 0)
            StartCoroutine(animatedTextureRenderLoop());

        Render();
    }

    public virtual void ServerInitialize()
    {
        if (averageRandomTickDuration != 0)
        {
            Chunk chunk = new ChunkPosition(location).GetChunk();

            chunk.randomTickBlocks.Add(this);
        }
    }

    public virtual void RandomTick()
    {
    }

    public virtual void BuildTick()
    {
        if (new ChunkPosition(location).GetChunk().isLoaded) //Block place sound
            Sound.Play(location, "block/" + blockSoundType.ToString().ToLower() + "/break", SoundType.Block, 0.5f
                , 1.5f);

        if ((rotateX || rotateY) && !(GetData().HasTag("rotated_x") || GetData().HasTag("rotated_y")))
            RotateTowardsPlayer();
    }

    public BlockData GetData()
    {
        return location.GetData();
    }

    protected Location SetData(BlockData data)
    {
        return location.SetData(data);
    }

    public virtual void GeneratingTick()
    {
    }

    public virtual void Tick()
    {
        UpdateColliders();
        RenderRotate();
    }

    private IEnumerator animatedTextureRenderLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(changeTextureTime);
            Render();
        }
    }

    public virtual void UpdateColliders()
    {
        GetComponent<Collider2D>().enabled = true;
        gameObject.layer = LayerMask.NameToLayer(solid || trigger ? "Block" : "NoCollision");

        GetComponent<Collider2D>().isTrigger = trigger;
        GetComponent<BoxCollider2D>().size =
            trigger
                ? new Vector2(0.9f, 0.9f)
                : new Vector2(1, 1); //Trigger has to be a little smaller than a block to avoid unintended triggering
    }

    public Color[] GetColorsInTexture()
    {
        Texture2D texture = GetSprite().texture;

        return texture.GetPixels();
    }

    public void RotateTowardsPlayer()
    {
        bool rotated_x = false;
        bool rotated_y = false;
        Player closestPlayer = (Player) Entity.ClosestEntityOfType(location, typeof(Player));
        
        if (closestPlayer == null)
            return;
        
        if (rotateY)
            rotated_y = (Player.localEntity.transform.position.y + 1) < location.y;
        if (rotateX)
            rotated_x = Player.localEntity.transform.position.x < location.x;

        BlockData newData = GetData();
        newData.SetTag("rotated_x", rotated_x ? "true" : "false");
        newData.SetTag("rotated_y", rotated_y ? "true" : "false");
        SetData(newData);

        RenderRotate();
    }

    public void RenderRotate()
    {
        bool rotated_x = false;
        bool rotated_y = false;

        rotated_x = GetData().GetTag("rotated_x") == "true";
        rotated_y = GetData().GetTag("rotated_y") == "true";

        GetComponent<SpriteRenderer>().flipX = rotated_x;
        GetComponent<SpriteRenderer>().flipY = rotated_y;
    }


    private IEnumerator repairBlockDamageOnceViable()
    {
        while (Time.time - timeOfLastHit < 1)
            yield return new WaitForSeconds(0.2f);

        blockHealth = breakTime;
    }
    
    public virtual void Break()
    {
        Break(true);
    }

    public virtual void Break(bool drop)
    {
        if (drop)
            Drop();

        Sound.Play(location, "block/" + blockSoundType.ToString().ToLower() + "/break", SoundType.Block, 0.5f, 1.5f);

        //Play block break effect to all clients
        new ChunkPosition(location).GetChunk().BlockBreakParticleEffect(location, GetColorsInTexture());
        
        location.SetMaterial(Material.Air).Tick();
    }

    public virtual void Drop()
    {
        GetDrop().Drop(location);
    }

    public virtual ItemStack GetDrop()
    {
        return new ItemStack(GetMaterial(), 1);
    }

    public virtual void Interact(PlayerInstance player)
    {
    }

    public virtual void Render()
    {
        GetComponent<SpriteRenderer>().sprite = GetSprite();
    }

    protected Sprite GetSprite()
    {
        return Resources.Load<Sprite>("Sprites/block/" + GetTexture());
    }

    public virtual string GetTexture()
    {
        if (randomTextures.Length > 0) 
            return GetRandomTexture();
        
        return GetMaterial().ToString();
    }

    private string GetRandomTexture()
    {
        //Default get a random alternative texture based on location
        int textureIndex = new Random(SeedGenerator.SeedByWorldLocation(location)).Next(0, randomTextures.Length);

        //Textures that change over time
        if (changeTextureTime > 0)
        {
            float totalTimePerTextureLoop = changeTextureTime * randomTextures.Length;
            textureIndex = (int) (Time.time % totalTimePerTextureLoop / changeTextureTime);
        }

        return randomTextures[textureIndex];
    }

    public virtual void Hit(PlayerInstance player, float time, Tool_Type tool_type = Tool_Type.None, Tool_Level tool_level = Tool_Level.None)
    {
        timeOfLastHit = Time.time;

        bool properToolStats = false;

        if (tool_level != Tool_Level.None && tool_type == properToolType && tool_level >= properToolLevel)
            time *= 2 + (float) tool_level * 2f;
        if (properToolLevel == Tool_Level.None ||
            tool_type == properToolType && tool_level >= properToolLevel)
            properToolStats = true;

        blockHealth -= time;

        Sound.Play(location, "block/" + blockSoundType.ToString().ToLower() + "/hit", SoundType.Block, 0.8f, 1.2f);

        if (!BreakIndicator.breakIndicators.ContainsKey(location))
            BreakIndicator.Spawn(location);

        if (blockHealth <= 0)
        {
            if (properToolStats)
                Break();
            else
                Break(false);

            player.playerEntity.GetComponent<Player>().DoToolDurability();

            return;
        }

        StartCoroutine(repairBlockDamageOnceViable());
    }
    
    public Material GetMaterial()
    {
        return (Material) Enum.Parse(typeof(Material), GetType().Name);
    }
}

}