using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mirror;
using UnityEngine;
using Random = System.Random;

[Serializable]
public class Entity : NetworkBehaviour
{
    public static List<Entity> entities = new List<Entity>();

    public static int MaxLivingAmount = 4;
    private Vector2 _cachedposition;

    //Entity data tags
    [EntityDataTag(false)] 
    public float age;
    [SyncVar] [EntityDataTag(false)] 
    public float fireTime;
    
    public bool dead;
    
    [EntityDataTag(false)] 
    public bool facingLeft;



    //Entity State
    [SyncVar]
    public int id;
    public bool isInLiquid;
    public bool isOnGround;
    [SyncVar]
    public bool isOnClimbable;
    [SyncVar] 
    public float portalTime;
    public bool portalCooldown;
    [SyncVar]
    public bool teleportingDimension;
    
    public Vector2 lastFramePosition;
    public GameObject burningRender;
    
    public static int EntityCount => entities.Count;
    public static int LivingEntityCount
    {
        get
        {
            var i = 0;
            foreach (var entity in entities)
                if (entity is LivingEntity)
                    i++;
            return i;
        }
    }

    //Entity properties
    public virtual bool ChunkLoadingEntity { get; } = false;

    public Location Location
    {
        get => Location.LocationByPosition(_cachedposition);
        set
        {
            transform.position = value.GetPosition();
        }
    }

    public virtual void Start()
    {
        UpdateCachedPosition();
        gameObject.name = "Entity [" + GetType().Name + "]";
        entities.Add(this);
        
        if(isServer)
            Initialize();
        if(isClient)
            ClientInitialize();
    }
    
    public virtual void Update()
    {
        if (dead)
            return;
        
        GetComponent<Rigidbody2D>().simulated = IsChunkLoaded();
        isInLiquid = (GetLiquidBlocksForEntity().Length > 0);
        UpdateCachedPosition();
        
        if(isServer)
            Tick();
        if(isClient)
            ClientUpdate();
    }

    [Server]
    public virtual void Initialize()
    {
        Load();
        if (!HasBeenSaved())
            Spawn();
    }

    [Server]
    public virtual void Spawn()
    {

    }

    [Server]
    public virtual void Tick()
    {
        if (ChunkLoadingEntity)
            Chunk.CreateChunksAround(new ChunkPosition(Location), Chunk.RenderDistance);
        
        age += Time.deltaTime;
        
        if (IsBurning())
        {
            ReduceFireTime();
            WaterRemoveFireTimeCheck();
        }

        CheckNetherPortal();
        CheckWaterSplash();
        CheckFireDamage();
        CheckVoidDamage();
        CheckSuffocation();
        CheckLavaDamage();
    }
    
    [Client]
    public virtual void ClientInitialize()
    {
    }

    [Client]
    public virtual void ClientUpdate()
    {
        GetRenderer().flipX = facingLeft;
        
        if (isInLiquid)
            isOnGround = false;
        
        CheckLightUpdate();
        DoFireRender();
    }

    [Server]
    public virtual void Teleport(Location loc)
    {
        Location = loc;
        ((LivingEntity)this).highestYlevelsinceground = 0;
    }
    
    [Server]
    public static Entity Spawn(string type)
    {
        if (Resources.Load("Entities/" + type) == null)
        {
            Debug.LogError("No Entity with the type '" + type + "' was found");
            return null;
        }

        GameObject obj = Instantiate((GameObject) Resources.Load("Entities/" + type));
        NetworkServer.Spawn(obj);
        var result = obj.GetComponent<Entity>();

        result.id = CreateId();

        return result;
    }
    
    public virtual void LateUpdate()
    {
        lastFramePosition = transform.position;
    }

    public void UpdateCachedPosition()
    {
        _cachedposition = transform.position;
    }

    public virtual bool IsBurning()
    {
        return (fireTime > 0);
    }

    public virtual bool IsChunkLoaded()
    {
        return new ChunkPosition(Location).IsChunkLoaded();
    }
    
    [Server]
    private void CheckWaterSplash()
    {
        if (isInLiquid && GetVelocity().y < -2)
        {
            bool isInWater = false;
            foreach (Liquid liquid in GetLiquidBlocksForEntity())
                if (liquid is Water)
                {
                    isInWater = true;
                    break;
                }

            if (isInWater)
            {
                Sound.Play(Location, "entity/water_splash", SoundType.Entities, 0.75f, 1.25f); //Play splash sound
                WaterSplashEffect();
            }
        }
    }

    [Server]
    private void WaterRemoveFireTimeCheck()
    {
        if(isInLiquid)
        {
            bool isInWater = false;
            foreach (Liquid liquid in GetLiquidBlocksForEntity())
                if (liquid is Water)
                {
                    isInWater = true;
                    break;
                }

            if (isInWater)
            {
                fireTime = 0;
            }
        }
    }
    
    [Server]
    private void ReduceFireTime()
    {
        fireTime -= Time.deltaTime;
    }

    [Server]
    private void CheckSuffocation()
    {
        if (!IsChunkLoaded())
            return;

        if ((Time.time % 0.5f) - Time.deltaTime <= 0)
        {
            var block = Location.GetBlock();

            if (block != null)
                if (block.solid && !block.trigger && !(block is Liquid))
                    TakeSuffocationDamage(1);
        }
    }
    
    [Server]
    private void CheckFireDamage()
    {
        if(IsBurning())
            if ((Time.time % 1f) - Time.deltaTime <= 0)
                TakeFireDamage(1);
    }

    [Server]
    public virtual void TakeFireDamage(float damage)
    {
        Damage(damage);
    }

    [Server]
    private void CheckVoidDamage()
    {
        if ((Time.time % 0.5f) - Time.deltaTime <= 0)
            if (transform.position.y < 0)
                TakeVoidDamage(2);
    }

    [Server]
    public virtual void TakeVoidDamage(float damage)
    {
        Damage(damage);
    }

    [Server]
    private void CheckLavaDamage()
    {
        if (isInLiquid)
        {
            bool isInLava = false;
            foreach(Liquid liquid in GetLiquidBlocksForEntity())
                if (liquid is Lava)
                {
                    isInLava = true;
                    break;
                }

            if (isInLava)
            {
                fireTime = 14;
                
                if ((Time.time % 0.5f) - Time.deltaTime <= 0)
                {
                    TakeLavaDamage(4);
                }
            }
        }
    }

    public Liquid[] GetLiquidBlocksForEntity()
    {
        List<Liquid> liquids = new List<Liquid>();
        foreach (Collider2D col in Physics2D.OverlapBoxAll(transform.position, GetComponent<BoxCollider2D>().size, 0))
            if (col.GetComponent<Liquid>() != null)
                liquids.Add(col.GetComponent<Liquid>());

        return liquids.ToArray();
    }

    [Server]
    public virtual void TakeLavaDamage(float damage)
    {
        Damage(damage);
    }
    
    [Server]
    public virtual void TakeSuffocationDamage(float damage)
    {
        Damage(damage);
    }

    public virtual List<ItemStack> GetDrops()
    {
        var result = new List<ItemStack>();

        return result;
    }

    [Server]
    public virtual void DropAllDrops()
    {
        var i = 0;
        foreach (var item in GetDrops())
            if (item.material != Material.Air && item.amount > 0)
            {
                var random = new Random((transform.position + "" + i).GetHashCode());
                var maxVelocity = new Vector2(2, 2);
                var dropPosition = Location + new Location(0, 2);

                item.Drop(dropPosition,
                    new Vector2((float) random.NextDouble() * (maxVelocity.x - -maxVelocity.x) + -maxVelocity.x,
                        (float) random.NextDouble() * (maxVelocity.x - -maxVelocity.x) + -maxVelocity.x));

                i++;
            }
    }

    [Server]
    public virtual void Die()
    {
        if (dead)
            return;
        
        DropAllDrops();
        DeleteOldSavePath();
        dead = true;
        
        StartCoroutine(ScheduleDestruction(0.25f));
    }

    IEnumerator ScheduleDestruction(float time)
    {
        yield return new WaitForSeconds(time);
        
        NetworkServer.Destroy(gameObject);
    }

    public void OnDestroy()
    {
        entities.Remove(this);
    }

    [Server]
    public virtual void Damage(float damage)
    {
        Die();
    }

    [Server]
    public virtual void Hit(float damage, Entity source)
    {
        TakeHitDamage(damage);
    }

    [Server]
    public virtual void TakeHitDamage(float damage)
    {
        Damage(damage);
    }

    public virtual void SetVelocity(Vector2 velocity)
    {
        GetComponent<Rigidbody2D>().velocity = velocity;
    }

    public virtual Vector2 GetVelocity()
    {
        return GetComponent<Rigidbody2D>().velocity;
    }

    [Server]
    public virtual List<string> GetSaveStrings()
    {
        var result = new List<string>();
        result.Add("location=" + JsonUtility.ToJson(Location));
        var fields = GetType().GetFields().Where(field => field.IsDefined(typeof(EntityDataTag), true));

        foreach (var field in fields)
        {
            foreach (var attribute in Attribute.GetCustomAttributes(field))
            {
                if (attribute is EntityDataTag)
                {
                    var json = ((EntityDataTag) attribute).json;

                    if (json)
                        result.Add(field.Name + "=" + JsonUtility.ToJson(field.GetValue(this)));
                    else
                        result.Add(field.Name + "=" + field.GetValue(this));
                }
            }
        }

        return result;
    }

    [Server]
    public virtual string SavePath()
    {
        return WorldManager.world.getPath() + "/chunks/" + Location.dimension + "/" +
               new ChunkPosition(Location).chunkX + "/entities/" + id + "." + GetType().Name;
    }

    [Server]
    public virtual void Save()
    {
        DeleteOldSavePath();

        var path = SavePath();

        if (!File.Exists(path))
            File.Create(path).Close();

        var lines = new List<string>();

        lines = GetSaveStrings();

        File.WriteAllLines(path, lines);
    }

    [Server]
    public virtual void DeleteOldSavePath()
    {
        File.Delete(SavePath());
    }

    [Server]
    public virtual void Unload()
    {
        Save();

        NetworkServer.Destroy(gameObject);
    }

    [Server]
    public virtual void Load()
    {
        if (!HasBeenSaved())
            return;

        var lines = new Dictionary<string, string>();

        foreach (var line in File.ReadAllLines(SavePath()))
            if (line.Contains("="))
                lines.Add(line.Split('=')[0], line.Split('=')[1]);

        if (lines.Count <= 1)
            return;

        Teleport(JsonUtility.FromJson<Location>(lines["location"]));
        var fields = GetType().GetFields().Where(field => field.IsDefined(typeof(EntityDataTag), true));

        foreach (var field in fields)
        {
            foreach (var attribute in Attribute.GetCustomAttributes(field))
            {
                if (attribute is EntityDataTag)
                {
                    var json = ((EntityDataTag) attribute).json;
                    var type = field.FieldType;

                    if (json)
                        field.SetValue(this, JsonUtility.FromJson(lines[field.Name], type));
                    else if (type == typeof(string))
                        field.SetValue(this, lines[field.Name]);
                    else
                        field.SetValue(this, Convert.ChangeType(lines[field.Name], type));
                }
            }
        }
    }

    [Server]
    public bool HasBeenSaved()
    {
        return File.Exists(SavePath());
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        if (col.transform.position.y + 1f < transform.position.y && Mathf.Abs(col.transform.position.x - transform.position.x) < 0.9f && !isInLiquid)
            isOnGround = true;
    }
    
    private void OnCollisionExit2D(Collision2D col)
    {
        if (col.transform.position.y + 1f < transform.position.y)
            isOnGround = false;
    }

    [Server]
    public static int CreateId()
    {
        return UnityEngine.Random.Range(1, 99999);
    }

    [Server]
    public void CheckNetherPortal()
    {
        List<Portal_Frame> portals = new List<Portal_Frame>();
        foreach (Collider2D col in Physics2D.OverlapBoxAll(transform.position, GetComponent<BoxCollider2D>().size, 0))
            if (col.GetComponent<Portal_Frame>() != null)
                portals.Add(col.GetComponent<Portal_Frame>());

        if (portals.Count == 0)
        {
            portalTime = 0;
            portalCooldown = false;
        }
        else if (!portalCooldown)
        {
            portalTime += Time.deltaTime;

            if (portalTime >= 3)
            {
                StartCoroutine(teleportNetherPortal());
                portalTime = 0;
                portalCooldown = true;
            }
        }
    }

    [Server]
    IEnumerator teleportNetherPortal()
    {
        teleportingDimension = true;
        
        //Translate dimension coordinates
        Dimension currentDimension = Location.dimension;
        Location newLocation = new Location(0, 300);

        if (currentDimension == Dimension.Overworld)
        {
            newLocation.x = Mathf.FloorToInt((float)Location.x / 8f);
            newLocation.dimension = Dimension.Nether;
        }
        else if (currentDimension == Dimension.Nether)
        {
            newLocation.x = Location.x * 8;
            newLocation.dimension = Dimension.Overworld;
        }

        //Load chunk in the other dimension
        ChunkPosition cPos = new ChunkPosition(newLocation);
        Chunk chunk = cPos.GetChunk();
        if (chunk == null)
            chunk = cPos.CreateChunk();

        //Wait for chunk to load
        while (!chunk.isLoaded)
            yield return new WaitForSeconds(0.5f);

        //Either get or create a portal in the new dimension
        Location portal;
        if (chunk.netherPortal != null)
            portal = chunk.netherPortal.location;
        else
            portal = chunk.GeneratePortal(newLocation.x);

        //Teleport player to the portal
        Teleport(portal);
        teleportingDimension = false;
    }

    [Client]
    private void CheckLightUpdate()
    {
        if (Vector2Int.FloorToInt(lastFramePosition) != Vector2Int.FloorToInt(Location.GetPosition()))
            UpdateClientLight();
    }

    [ClientRpc]
    public virtual void WaterSplashEffect()
    {
        if (GetLiquidBlocksForEntity().Length == 0)
            return;
        
        var r = new Random();
        for (var i = 0; i < 8; i++) //Spawn landing partickes
        {
            Particle part = Particle.Spawn();

            part.transform.position = Location.GetPosition() + new Vector2(0, 0.5f);
            part.color = GetLiquidBlocksForEntity()[0].GetRandomColourFromTexture();
            part.doGravity = true;
            part.velocity = new Vector2(
                (1f + (float) r.NextDouble()) * (r.Next(0, 2) == 0 ? -1 : 1)
                , 3f + (float) r.NextDouble());
            part.maxAge = 1f + (float) r.NextDouble();
            part.maxBounces = 10;
        }
    }

    [Client]
    private void UpdateClientLight()
    {
        LightObject lightObj = GetRenderer().GetComponent<LightObject>();

        if (lightObj != null)
            LightManager.UpdateLightObject(lightObj);
    }
    
    [Client]
    public virtual SpriteRenderer GetRenderer()
    {
        return transform.Find("_renderer").GetComponent<SpriteRenderer>();
    }
    
    [ClientRpc]
    public void CriticalDamageEffect()
    {
        var r = new Random();
        for (var i = 0; i < r.Next(2, 8); i++) //SpawnParticles
        {
            Particle part = Particle.Spawn();

            part.transform.position = Location.GetPosition() + new Vector2(0, 1f);
            part.color = new Color(0.854f, 0.788f, 0.694f);
            part.doGravity = true;
            part.velocity = new Vector2(
                (2f + (float)r.NextDouble()) * (r.Next(0, 2) == 0 ? -1 : 1)
                , 4f + (float)r.NextDouble());
            part.maxAge = 1f + (float)r.NextDouble();
            part.maxBounces = 10;
        }
    }
    
    [Client]
    private void DoFireRender()
    {
        if(burningRender != null)
            burningRender.SetActive(IsBurning());
    }
}