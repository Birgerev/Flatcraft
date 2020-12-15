using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using Random = System.Random;

[BurstCompile]
public class Block : MonoBehaviour
{
    public int age;

    public float blockHealth;

    public BlockData data = new BlockData();

    public Location location;

    public string texture;
    private float time_of_last_autosave;

    private float time_of_last_hit;
    public virtual string[] alternative_textures { get; } = { };
    public virtual float change_texture_time { get; } = 0;

    public virtual bool solid { get; set; } = true;
    public virtual bool isFlammable { get; } = false;
    public virtual bool triggerCollider { get; } = false;
    public virtual bool requiresGround { get; } = false;
    public virtual bool autosave { get; } = false;
    public virtual bool autoTick { get; } = false;
    public virtual float averageRandomTickDuration { get; } = 0;
    public virtual float breakTime { get; } = 0.75f;
    public virtual bool rotate_x { get; } = false;
    public virtual bool rotate_y { get; } = false;

    public virtual Tool_Type propperToolType { get; } = Tool_Type.None;
    public virtual Tool_Level propperToolLevel { get; } = Tool_Level.None;

    public virtual Block_SoundType blockSoundType { get; } = Block_SoundType.Stone;

    public virtual int glowLevel { get; } = 0;

    public void ScheduleBlockInitialization()
    {
        StartCoroutine(scheduleBlockInitialization());
    }

    private IEnumerator scheduleBlockInitialization()
    {
        yield return new WaitForSeconds(0.02f);
        Initialize();
    }

    public void ScheduleBlockBuildTick()
    {
        StartCoroutine(scheduleBlockBuildTick());
    }

    private IEnumerator scheduleBlockBuildTick()
    {
        yield return new WaitForSeconds(0.02f);
        BuildTick();
    }

    public void ScheduleBlockTick()
    {
        StartCoroutine(scheduleBlockTick());
    }

    private IEnumerator scheduleBlockTick()
    {
        yield return new WaitForSeconds(0.03f);
        Tick();
    }

    public virtual void Initialize()
    {
        blockHealth = breakTime;

        texture = (string) GetType().GetField("default_texture", BindingFlags.Public | BindingFlags.Static)
            .GetValue(null);


        UpdateColliders();

        //Cache position for use in multithreading
        location = Location.LocationByPosition(transform.position, location.dimension);
        
        if (glowLevel > 0)
        {
            GameObject lightSource = Instantiate(LightManager.instance.lightSourcePrefab, transform);
            lightSource.transform.localPosition = Vector3.zero;
            lightSource.GetComponent<LightSource>().UpdateLightLevel(glowLevel);
        }

        if (autoTick || autosave)
            StartCoroutine(autoTickLoop());
        if (averageRandomTickDuration != 0)
            StartCoroutine(randomTickLoop());

        if (change_texture_time != 0)
            StartCoroutine(animatedTextureRenderLoop());

        Render();
    }

    public virtual void RandomTick()
    {
    }

    public virtual void BuildTick()
    {
        if ((rotate_x || rotate_y) && !(data.HasData("rotated_x") || data.HasData("rotated_y")))
        {
            RotateTowardsPlayer();
        }
    }

    public virtual void GeneratingTick()
    {
    }

    public virtual void Tick()
    {
        if (age == 0 && new ChunkPosition(location).GetChunk().isLoaded) //Block place sound
            Sound.Play(location, "block/" + blockSoundType.ToString().ToLower() + "/break", SoundType.Blocks, 0.5f,
                1.5f);

        checkGround();
        UpdateColliders();
        RenderRotate();


        if (Time.time - time_of_last_autosave > SaveManager.AutosaveDuration && autosave)
        {
            Autosave();
            return;
        }

        age++;
    }

    private void checkGround()
    {
        if (requiresGround)
            if ((location - new Location(0, 1)).GetMaterial() == Material.Air)
                Break();
    }

    public float getRandomChance()
    {
        return (float) new Random(SeedGenerator.SeedByLocation(location) + age).NextDouble();
    }

    private IEnumerator animatedTextureRenderLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(change_texture_time);
            Render();
        }
    }
    
    private IEnumerator randomTickLoop()
    {
        var r = new Random(SeedGenerator.SeedByLocation(location));

        while (true)
        {
            float nextTickDuration = 1;
            while (r.NextDouble() > 1 / averageRandomTickDuration) nextTickDuration += 1;

            yield return new WaitForSeconds(nextTickDuration);
            RandomTick();
        }
    }

    private IEnumerator autoTickLoop()
    {
        //Wait a random duration, to smooth out ticks across time
        yield return new WaitForSeconds((float) new Random(SeedGenerator.SeedByLocation(location)).NextDouble() *
                                        (1f / Chunk.TickRate));

        while (true)
        {
            yield return new WaitForSeconds(1 / Chunk.TickRate);
            Tick();
        }
    }

    public virtual void UpdateColliders()
    {
        UpdateColliders(solid, triggerCollider);
    }

    public virtual void UpdateColliders(bool collide, bool trigger)
    {
        gameObject.layer = LayerMask.NameToLayer(collide ? "Block" : "NoCollisionBlock");
        GetComponent<Collider2D>().isTrigger = trigger;
    }

    public Color GetRandomColourFromTexture()
    {
        var texture = getTexture().texture;
        var pixels = texture.GetPixels();
        var random = new Random(DateTime.Now.GetHashCode());

        return pixels[random.Next(pixels.Length)];
    }

    public void RotateTowardsPlayer()
    {
        var rotated_x = false;
        var rotated_y = false;

        if (rotate_y) rotated_y = Player.localInstance.transform.position.y < location.y;
        if (rotate_x) rotated_x = Player.localInstance.transform.position.x < location.x;

        data.SetData("rotated_x", rotated_x ? "true" : "false");
        data.SetData("rotated_y", rotated_y ? "true" : "false");

        //Save new rotation
        Autosave();
        RenderRotate();
    }

    public void RenderRotate()
    {
        var rotated_x = false;
        var rotated_y = false;

        rotated_x = data.GetData("rotated_x") == "true";
        rotated_y = data.GetData("rotated_y") == "true";

        GetComponent<SpriteRenderer>().flipX = rotated_x;
        GetComponent<SpriteRenderer>().flipY = rotated_y;
    }

    public virtual void Autosave()
    {
        time_of_last_autosave = Time.time;

        location.SetData(data);
    }

    public virtual void Hit(float time)
    {
        Hit(time, Tool_Type.None, Tool_Level.None);
    }

    public virtual void Hit(float time, Tool_Type tool_type, Tool_Level tool_level)
    {
        time_of_last_hit = Time.time;

        var properToolStats = false;

        if (tool_level != Tool_Level.None && tool_type == propperToolType && tool_level >= propperToolLevel)
            time *= 2 + (float) tool_level * 2f;
        if (propperToolLevel == Tool_Level.None ||
            tool_type == propperToolType && tool_level >= propperToolLevel)
            properToolStats = true;

        blockHealth -= time;

        Sound.Play(location, "block/" + blockSoundType.ToString().ToLower() + "/hit", SoundType.Blocks, 0.8f, 1.2f);

        RenderBlockDamage();

        if (blockHealth <= 0)
        {
            if (properToolStats)
                Break();
            else
                Break(false);

            Player.localInstance.DoToolDurability();

            return;
        }

        RenderBlockDamage();
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

        var r = new Random();
        for (var i = 0; i < r.Next(2, 8); i++) //Spawn Particles
        {
            var part = (Particle) Entity.Spawn("Particle");

            part.transform.position = location.GetPosition() +
                                      new Vector2((float) r.NextDouble() - 0.5f, (float) r.NextDouble() - 0.5f);
            part.color = GetRandomColourFromTexture();
            part.doGravity = true;
            part.velocity = Vector2.zero;
            part.maxAge = 1f + (float) r.NextDouble();
            part.maxBounces = 10;
        }

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

    public virtual void Interact()
    {
    }

    public void ResetBlockDamage()
    {
        blockHealth = breakTime;
        RenderBlockDamage();
    }

    public virtual void RenderBlockDamage()
    {
        var damageIndicator = transform.Find("BreakIndicator");

        if (blockHealth != breakTime)
        {
            var sprites = Resources.LoadAll<Sprite>("Sprites/Block_Break");
            var spriteIndex = (int) (blockHealth / breakTime / (1f / sprites.Length));

            BreakIndicator.instance.UpdateState(spriteIndex, location);
        }
    }

    public virtual void Render()
    {
        GetComponent<SpriteRenderer>().sprite = getTexture();
    }

    public virtual Sprite getTexture()
    {
        if (change_texture_time > 0 && alternative_textures.Length > 0)
        {
            var totalTimePerTextureLoop = change_texture_time * alternative_textures.Length;
            var textureIndex = (int) (Time.time % totalTimePerTextureLoop / change_texture_time);

            return Resources.Load<Sprite>("Sprites/" + alternative_textures[textureIndex]);
        }

        if (alternative_textures.Length > 0)
        {
            var textureIndex = new Random(SeedGenerator.SeedByLocation(location)).Next(0, alternative_textures.Length);

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
    Stone,
    Wood,
    Sand,
    Dirt,
    Grass,
    Wool,
    Gravel
}