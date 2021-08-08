﻿using System;
using System.Collections;
using Unity.Burst;
using UnityEngine;
using Random = System.Random;

[BurstCompile]
public class Block : MonoBehaviour
{

    public float blockHealth;

    public Location location;

    private float time_of_last_hit;

    public virtual string texture { get; set; } = "";
    public virtual string[] alternative_textures { get; } = { };
    public virtual float change_texture_time { get; } = 0;

    public virtual bool solid { get; set; } = true;
    public virtual bool isFlammable { get; } = false;
    public virtual bool trigger { get; set; } = false;
    public virtual bool climbable { get; } = false;
    public virtual bool requiresGround { get; } = false;
    public virtual float averageRandomTickDuration { get; } = 0;
    public virtual float breakTime { get; } = 0.75f;
    public virtual bool rotate_x { get; } = false;
    public virtual bool rotate_y { get; } = false;

    public virtual Tool_Type propperToolType { get; } = Tool_Type.None;
    public virtual Tool_Level propperToolLevel { get; } = Tool_Level.None;

    public virtual Block_SoundType blockSoundType { get; } = Block_SoundType.Stone;

    public virtual int glowLevel { get; } = 0;

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

        if (glowLevel > 0)
        {
            LightSource source = LightSource.Create(transform);

            source.UpdateLightLevel(glowLevel, true);
        }

        if (change_texture_time != 0)
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
            Sound.Play(location, "block/" + blockSoundType.ToString().ToLower() + "/break", SoundType.Blocks, 0.5f
                , 1.5f);

        if ((rotate_x || rotate_y) && !(GetData().HasTag("rotated_x") || GetData().HasTag("rotated_y")))
            RotateTowardsPlayer();
    }

    public BlockData GetData()
    {
        return location.GetData();
    }

    protected void SetData(BlockData data)
    {
        location.SetData(data);
    }

    public virtual void GeneratingTick()
    {
    }

    public virtual void Tick()
    {
        CheckGround();
        UpdateColliders();
        RenderRotate();
    }

    private void CheckGround()
    {
        if (requiresGround)
            if ((location - new Location(0, 1)).GetMaterial() == Material.Air)
                Break();
    }

    private IEnumerator animatedTextureRenderLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(change_texture_time);
            Render();
        }
    }

    public virtual void UpdateColliders()
    {
        GetComponent<Collider2D>().enabled = true;
        gameObject.layer = LayerMask.NameToLayer(solid || trigger ? "Block" : "NoCollisionBlock");

        GetComponent<Collider2D>().isTrigger = trigger;
        GetComponent<BoxCollider2D>().size =
            trigger
                ? new Vector2(0.9f, 0.9f)
                : new Vector2(1, 1); //Trigger has to be a little smaller than a block to avoid unintended triggering
    }

    public Color GetRandomColourFromTexture()
    {
        Texture2D texture = getTexture().texture;
        Color[] pixels = texture.GetPixels();
        Random random = new Random(DateTime.Now.GetHashCode());

        return pixels[random.Next(pixels.Length)];
    }

    public void RotateTowardsPlayer()
    {
        bool rotated_x = false;
        bool rotated_y = false;

        if (rotate_y)
            rotated_y = Player.localEntity.transform.position.y < location.y;
        if (rotate_x)
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

    public virtual void Hit(PlayerInstance player, float time)
    {
        Hit(player, time, Tool_Type.None, Tool_Level.None);
    }

    public virtual void Hit(PlayerInstance player, float time, Tool_Type tool_type, Tool_Level tool_level)
    {
        time_of_last_hit = Time.time;

        bool properToolStats = false;

        if (tool_level != Tool_Level.None && tool_type == propperToolType && tool_level >= propperToolLevel)
            time *= 2 + (float) tool_level * 2f;
        if (propperToolLevel == Tool_Level.None ||
            tool_type == propperToolType && tool_level >= propperToolLevel)
            properToolStats = true;

        blockHealth -= time;

        Sound.Play(location, "block/" + blockSoundType.ToString().ToLower() + "/hit", SoundType.Blocks, 0.8f, 1.2f);

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

    private IEnumerator repairBlockDamageOnceViable()
    {
        while (Time.time - time_of_last_hit < 1)
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

        Sound.Play(location, "block/" + blockSoundType.ToString().ToLower() + "/break", SoundType.Blocks, 0.5f, 1.5f);

        Random r = new Random();
        for (int i = 0; i < r.Next(2, 8); i++) //Spawn Particles
        {
            Particle part = Particle.Spawn();

            part.transform.position = location.GetPosition() +
                                      new Vector2((float) r.NextDouble() - 0.5f, (float) r.NextDouble() - 0.5f);
            part.color = GetRandomColourFromTexture();
            part.doGravity = true;
            part.velocity = Vector2.zero;
            part.maxAge = 1f + (float) r.NextDouble();
            part.maxBounces = 10;
        }

        if (GetComponentInChildren<LightSource>() != null)
            LightManager.DestroySource(GetComponentInChildren<LightSource>());

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
        GetComponent<SpriteRenderer>().sprite = getTexture();
    }

    public virtual Sprite getTexture()
    {
        if (change_texture_time > 0 && alternative_textures.Length > 0)
        {
            float totalTimePerTextureLoop = change_texture_time * alternative_textures.Length;
            int textureIndex = (int) (Time.time % totalTimePerTextureLoop / change_texture_time);

            return Resources.Load<Sprite>("Sprites/" + alternative_textures[textureIndex]);
        }

        if (alternative_textures.Length > 0)
        {
            int textureIndex = new Random(SeedGenerator.SeedByLocation(location)).Next(0, alternative_textures.Length);

            return Resources.Load<Sprite>("Sprites/" + alternative_textures[textureIndex]);
        }

        return Resources.Load<Sprite>("Sprites/" + texture);
    }

    public Material GetMaterial()
    {
        return (Material) Enum.Parse(typeof(Material), GetType().Name);
    }
}

public enum Block_SoundType
{
    Stone
    , Wood
    , Sand
    , Dirt
    , Grass
    , Wool
    , Gravel
    , Ladder
    , Glass
    , Fire
}