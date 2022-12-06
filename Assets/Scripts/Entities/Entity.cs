using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mirror;
using UnityEngine;
using Random = System.Random;

[Serializable]
public class Entity : NetworkBehaviour
{
    public static List<Entity> entities = new List<Entity>();
    
    //Entity data tags
    [EntityDataTag(false)] public float age;

    [SyncVar] [EntityDataTag(false)] public float fireTime;

    public bool dead;

    [SyncVar] [EntityDataTag(false)] public bool facingLeft;


    //Entity State
    [SyncVar] public string uuid;

    public bool isInLiquid;
    public bool isOnGround;

    [SyncVar] public bool isOnClimbable;

    [SyncVar] public float portalTime;

    public bool portalCooldown;

    [SyncVar] public bool teleportingDimension;

    public Entity lastDamager;

    public Vector2 lastFramePosition;
    public GameObject burningRender;
    private Vector2 _cachedposition;
    private bool inLiquidLastFrame;

    public static int EntityCount => entities.Count;

    public static int LivingEntityCount
    {
        get
        {
            int i = 0;
            foreach (Entity entity in entities)
                if (entity is LivingEntity)
                    i++;
            return i;
        }
    }

    //Entity properties
    public virtual bool ChunkLoadingEntity { get; }

    public Location Location
    {
        get => Location.LocationByPosition(_cachedposition);
        set => transform.position = value.GetPosition();
    }

    public virtual void Start()
    {
        UpdateCachedPosition();
        gameObject.name = "Entity [" + GetType().Name + "]";
        entities.Add(this);

        if (isServer)
        {
            if (!HasBeenSaved())
                Spawn();

            Initialize();
        }

        if (isClient)
            ClientInitialize();
    }

    public virtual void Update()
    {
        if (dead)
            return;

        if (GetComponent<Rigidbody2D>() != null)
            GetComponent<Rigidbody2D>().simulated = IsChunkLoaded();
        isInLiquid = GetLiquidBlocksForEntity().Length > 0;
        UpdateCachedPosition();

        CheckOnGround();
        if (isServer)
            Tick();
        if (isClient)
            ClientUpdate();
    }

    public virtual void LateUpdate()
    {
        lastFramePosition = transform.position;
        inLiquidLastFrame = isInLiquid;
    }

    [Server]
    public virtual void Initialize()
    {
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
        UpdateClientLight();
    }

    [Client]
    public virtual void ClientUpdate()
    {
        //Mirror renderer direction if facingLeft doesnt match current render facing direction
        if ((GetRenderer().transform.localScale.x < 0) != facingLeft)
            GetRenderer().transform.localScale *= new Vector2(-1, 1); 
        

        if (isInLiquid)
            isOnGround = false;

        CheckLightUpdate();
        DoFireRender();
    }

    [Server]
    public virtual void Teleport(Location loc)
    {
        Location = loc;
        UpdateCachedPosition();
    }

    [Server]
    public static Entity Spawn(string type)
    {
        return Spawn(type, CreateUUID(), Vector2.zero);
    }

    [Server]
    public static Entity Spawn(string type, string uuid, Vector2 pos)
    {
        if (Resources.Load("Entities/" + type) == null)
        {
            Debug.LogError("No Entity with the type '" + type + "' was found");
            return null;
        }

        GameObject obj = Instantiate((GameObject) Resources.Load("Entities/" + type));
        Entity entity = obj.GetComponent<Entity>();

        entity.transform.position = pos;
        entity.UpdateCachedPosition();
        entity.uuid = uuid;
        entity.Load();

        NetworkServer.Spawn(obj);

        return entity;
    }

    public void UpdateCachedPosition()
    {
        _cachedposition = transform.position;
    }

    public virtual bool IsBurning()
    {
        return fireTime > 0;
    }

    public virtual bool IsChunkLoaded()
    {
        return new ChunkPosition(Location).IsChunkLoaded();
    }
    
    private void CheckOnGround()
    {
        Collider2D collider = GetComponent<Collider2D>();
        Vector2 pos = (Vector2)transform.position + new Vector2(collider.offset.x, -0.05f);
        Vector2 size = new Vector2(collider.bounds.extents.x * 2, 0.1f);
        
        ContactFilter2D filter = new ContactFilter2D().NoFilter();
        filter.SetLayerMask(LayerMask.GetMask("Block"));
        
        Collider2D[] blockBeneathColliders = Physics2D.OverlapBoxAll(pos, size, 0, filter.layerMask.value);
        foreach (Collider2D col in blockBeneathColliders)
        {
            Block block = col.GetComponent<Block>();
            if (block != null && block.solid && !block.trigger)
            {
                isOnGround = true;
                return;
            }
        }
        

        isOnGround = false;
    }
    
    [Server]
    private void CheckWaterSplash()
    {
        if (isInLiquid && !inLiquidLastFrame && GetVelocity().y < - 2)
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
        if (isInLiquid)
        {
            bool isInWater = false;
            foreach (Liquid liquid in GetLiquidBlocksForEntity())
                if (liquid is Water)
                {
                    isInWater = true;
                    break;
                }

            if (isInWater)
                fireTime = 0;
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

        if (Time.time % 0.5f - Time.deltaTime <= 0)
        {
            foreach (Block block in GetBlocksForEntity())
            {
                if (block.solid && !block.trigger && !(block is Liquid))
                {
                    TakeSuffocationDamage(1);
                    return;
                }
            }
        }
    }

    [Server]
    private void CheckFireDamage()
    {
        if (IsBurning())
            if (Time.time % 1f - Time.deltaTime <= 0)
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
        if (Time.time % 0.5f - Time.deltaTime <= 0)
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
            foreach (Liquid liquid in GetLiquidBlocksForEntity())
                if (liquid is Lava)
                {
                    isInLava = true;
                    break;
                }

            if (isInLava)
            {
                fireTime = 14;

                if (Time.time % 0.5f - Time.deltaTime <= 0)
                    TakeLavaDamage(4);
            }
        }
    }

    public Liquid[] GetLiquidBlocksForEntity()
    {
        List<Liquid> liquids = new List<Liquid>();
        foreach (Block block in GetBlocksForEntity())
            if (block is Liquid)
                liquids.Add((Liquid)block);

        return liquids.ToArray();
    }
    
    public Block[] GetBlocksForEntity()
    {
        List<Block> blocks = new List<Block>();
        Vector2 offset = (Vector2) transform.position + GetComponent<BoxCollider2D>().offset;
        Vector2 size = GetComponent<BoxCollider2D>().size * 0.9f;
        foreach (Collider2D col in Physics2D.OverlapBoxAll(offset, size, 0))
            if (col.GetComponent<Block>() != null)
                blocks.Add(col.GetComponent<Block>());

        return blocks.ToArray();
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
        List<ItemStack> result = new List<ItemStack>();

        return result;
    }

    [Server]
    public virtual void DropAllDrops()
    {
        int i = 0;
        foreach (ItemStack item in GetDrops())
            if (item.material != Material.Air && item.Amount > 0)
            {
                Random random = new Random((transform.position + "" + i).GetHashCode());
                Vector2 maxVelocity = new Vector2(2, 2);

                item.Drop(Location,
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
        dead = true;

        StartCoroutine(ScheduleDestruction(0.25f));
    }

    private IEnumerator ScheduleDestruction(float time)
    {
        yield return new WaitForSeconds(time);

        Remove();
    }

    [Server]
    public virtual void Remove()
    {
        dead = true;
        entities.Remove(this);
        NetworkServer.Destroy(gameObject);
    }
    
    [Server]
    public virtual void Damage(float damage)
    {
        Die();
    }

    [Server]
    public virtual void Hit(float damage, Entity source)
    {
        lastDamager = source;

        if (source.fireTime > 0)
            fireTime = 7;
        
        TakeHitDamage(damage);
    }
    
    [Server]
    public virtual void Interact(Player source)
    {
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
        List<string> result = new List<string>();
        result.Add("location=" + JsonUtility.ToJson(Location));
        IEnumerable<FieldInfo> fields =
            GetType().GetFields().Where(field => field.IsDefined(typeof(EntityDataTag), true));

        foreach (FieldInfo field in fields)
        foreach (Attribute attribute in Attribute.GetCustomAttributes(field))
            if (attribute is EntityDataTag)
            {
                bool json = ((EntityDataTag) attribute).json;

                if (json)
                    result.Add(field.Name + "=" + JsonUtility.ToJson(field.GetValue(this)));
                else
                    result.Add(field.Name + "=" + field.GetValue(this));
            }

        return result;
    }

    [Server]
    public virtual string SavePath()
    {
        return WorldManager.world.GetPath() + "/chunks/" + Location.dimension + "/" +
               new ChunkPosition(Location).chunkX + "/entities/" + uuid + "." + GetType().Name;
    }

    [Server]
    public virtual void Save()
    {
        if (dead)
            return;

        string path = SavePath();

        if (!File.Exists(path))
            File.Create(path).Close();

        List<string> lines = GetSaveStrings();

        File.WriteAllLines(path, lines);
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

        Dictionary<string, string> lines = new Dictionary<string, string>();

        foreach (string line in File.ReadAllLines(SavePath()))
            if (line.Contains("="))
                lines.Add(line.Split('=')[0], line.Split('=')[1]);

        if (lines.Count <= 1)
            return;

        Teleport(JsonUtility.FromJson<Location>(lines["location"]));
        IEnumerable<FieldInfo> fields =
            GetType().GetFields().Where(field => field.IsDefined(typeof(EntityDataTag), true));

        foreach (FieldInfo field in fields)
        {
            foreach (Attribute attribute in Attribute.GetCustomAttributes(field))
            {
                try
                {
                    if (attribute is EntityDataTag)
                    {
                        bool json = ((EntityDataTag) attribute).json;
                        Type type = field.FieldType;

                        if (json)
                            field.SetValue(this, JsonUtility.FromJson(lines[field.Name], type));
                        else if (type == typeof(string))
                            field.SetValue(this, lines[field.Name]);
                        else
                            field.SetValue(this, Convert.ChangeType(lines[field.Name], type));
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Error in loading field for entity: '" + this.GetType() + "', field: '" + field.Name + "'.   " +e.Message + e.StackTrace);
                }
                
            }
        }
    }

    [Server]
    public bool HasBeenSaved()
    {
        return File.Exists(SavePath());
    }

    [Server]
    public static string CreateUUID()
    {
        Random random = new Random(Guid.NewGuid().GetHashCode());
        int uuidLength = 32;
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

        return new string(Enumerable.Range(1, uuidLength)
            .Select(_ => chars[random.Next(chars.Length)]).ToArray());
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
                //Center player at portal
                Location = portals[0].location;
                
                StartCoroutine(teleportNetherPortal());
                portalTime = 0;
                portalCooldown = true;
            }
        }
    }

    [Server]
    private IEnumerator teleportNetherPortal()
    {
        teleportingDimension = true;

        //Translate dimension coordinates
        Dimension currentDimension = Location.dimension;
        Location newLocation = new Location(0, 300);

        if (currentDimension == Dimension.Overworld)
        {
            newLocation.x = Mathf.FloorToInt(Location.x / 8f);
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

        Random r = new Random();
        for (int i = 0; i < 8; i++) //Spawn landing partickes
        {
            Particle part = Particle.ClientSpawn();

            part.transform.position = Location.GetPosition() + new Vector2(0, 0.5f);
            part.color = GetLiquidBlocksForEntity()[0].GetRandomColourFromTexture();
            part.doGravity = true;
            part.velocity = new Vector2((1f + (float) r.NextDouble()) * (r.Next(0, 2) == 0 ? -1 : 1)
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
        Random r = new Random();
        for (int i = 0; i < r.Next(2, 8); i++) //SpawnParticles
        {
            Particle part = Particle.ClientSpawn();

            part.transform.position = Location.GetPosition() + new Vector2(0, 1f);
            part.color = new Color(0.854f, 0.788f, 0.694f);
            part.doGravity = true;
            part.velocity = new Vector2((2f + (float) r.NextDouble()) * (r.Next(0, 2) == 0 ? -1 : 1)
                , 4f + (float) r.NextDouble());
            part.maxAge = 1f + (float) r.NextDouble();
            part.maxBounces = 10;
        }
    }

    public static Entity GetEntity(string uuid)
    {
        foreach (Entity e in entities)
        {
            if (e.uuid.Equals(uuid))
            {
                return e;
            }
        }

        return null;
    }
    
    public static Entity ClosestEntityOfType(Location loc, Type type)
    {
        Entity closestEntity = null;
        float closestEntityDistance = int.MaxValue;
        foreach (Entity entity in entities)
        {
            if(!type.IsInstanceOfType(entity))
                continue;
            
            float distance = Vector2.Distance(entity.Location.GetPosition(), loc.GetPosition());
            if (!(distance < closestEntityDistance))
                continue;
            
            closestEntity = entity;
            closestEntityDistance = distance;
        }

        return closestEntity;
    }
    
    [Client]
    private void DoFireRender()
    {
        if (burningRender != null)
            burningRender.SetActive(IsBurning());
    }
    
    public float GetWidth()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();

        if (col == null)
            return 0;

        return col.bounds.size.x + col.edgeRadius;
    }
    
    public float GetHeight()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();

        if (col == null)
            return 0;

        return col.bounds.size.y + col.edgeRadius;
    }
    
    public static ContactFilter2D GetFilter()
    {
        ContactFilter2D filter = new ContactFilter2D().NoFilter();
        filter.SetLayerMask(LayerMask.GetMask("Player", "Entity", "DroppedItem", "FallingBlock", "Painting", "Particle", "Projectile"));

        return filter;
    }
}