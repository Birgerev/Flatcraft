using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Mirror;
using UnityEngine;
using Random = System.Random;

public class Player : HumanEntity
{
    public static float interactionsPerPerSecond = 4.5f;

    //Entity State
    public static Player localEntity;
    public static List<Player> players = new List<Player>();

    [SyncVar] public GameObject playerInstance;
    public GameObject crosshair;

    //Entity Data Tags
    [EntityDataTag(false)] [SyncVar] public float hunger;

    [EntityDataTag(false)] [SyncVar] public int inventoryId = 0;

    [SyncVar] public float eatingTime;

    public Location bedLocation = new Location(0, 0);
    public float maxHunger = 20;
    public float reach = 5;

    [SyncVar] public bool sprinting;

    [SyncVar] public bool sneaking;

    private Material actionBarLastSelectedMaterial;
    private int framesSinceInventoryOpen;
    private readonly float healthRegenerationHungerCost = 0.4f;
    private readonly float jumpHungerCost = 0.1f;
    private bool ladderSneaking;

    [SyncVar] private double lastBlockHitTime;

    private float lastBlockInteractionTime;
    private float lastFrameScroll;
    private float lastDoubleTapSprintTap;

    [SyncVar] private double lastHitTime;

    private readonly float movementHungerCost = 0.03f;
    private readonly float sneakSpeed = 1.3f;
    private readonly float sprintHungerCost = 0.03f;
    private readonly float sprintSpeed = 5.6f;

    //Entity Properties
    public override bool ChunkLoadingEntity { get; } = true;
    public override float maxHealth { get; } = 20;

    public override void Start()
    {
        Debug.Log("Spawning player '" + uuid + "'");
        players.Add(this);

        base.Start();
    }

    public void OnDestroy()
    {
        players.Remove(this);
    }

    [Client]
    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        localEntity = this;
        CameraController.instance.target = transform;
    }

    [Server]
    public override void Spawn()
    {
        base.Spawn();

        hunger = maxHunger;

        if (bedLocation.GetMaterial() == Material.Bed_Bottom || bedLocation.GetMaterial() == Material.Bed_Top)
            Teleport(bedLocation);

        StartCoroutine(ValidSpawnOnceChunkLoaded());
    }

    [Server]
    public override void Tick()
    {
        base.Tick();

        if (inventoryId == 0) //Make sure player has an inventory
        {
            Inventory inv = PlayerInventory.CreatePreset();
            inventoryId = inv.id;
        }

        CalculateFlip();
        GetInventory().holder = Location;
        CheckHunger();
        CheckRegenerateHealth();
        CheckStarvationDamage();
        ClimbableSound();

        //Sprint particles    
        if (sprinting && isOnGround)
            MovementParticlesEffect(0.2f);
    }

    [Client]
    public override void ClientUpdate()
    {
        //Crosshair
        if (hasAuthority)
        {
            PerformInput();
            CheckActionBarUpdate();
            CheckDimensionChangeLoadingScreen();

            if (!isServer) //If we are server, Process movement will already have been called in LivingEntity.Tick()
                ProcessMovement();

            if (Inventory.IsAnyOpen(playerInstance.GetComponent<PlayerInstance>()))
                framesSinceInventoryOpen = 0;
            else
                framesSinceInventoryOpen++;
        }
        
        base.ClientUpdate();
    }

    [Server]
    public override void Teleport(Location loc)
    {
        TeleportOwningClientPlayer(loc);
        base.Teleport(loc);
    }

    [ClientRpc]
    public void TeleportOwningClientPlayer(Location loc)
    {
        if (hasAuthority)
            Location = loc;
    }

    public override void ProcessMovement()
    {
        if (!hasAuthority)
            return;

        base.ProcessMovement();
        CrouchOnLadderCheck();
    }

    public void CrouchOnLadderCheck()
    {
        bool isLadderSneakingThisFrame = isOnClimbable && sneaking;
        if (isLadderSneakingThisFrame && !ladderSneaking)
        {
            GetComponent<Rigidbody2D>().gravityScale = 0;
            ladderSneaking = true;
        }
        else if (!isLadderSneakingThisFrame && ladderSneaking)
        {
            GetComponent<Rigidbody2D>().gravityScale = 1;
            ladderSneaking = false;
        }
    }

    [Client]
    public override void UpdateNameplate()
    {
        if (hasAuthority)
        {
            nameplate.text = "";
            return;
        }

        base.UpdateNameplate();
    }

    [Server]
    private void CheckRegenerateHealth()
    {
        if (health >= 20)
            return;

        if (hunger > 19)
        {
            if (Time.time % 0.5f - Time.deltaTime <= 0)
            {
                health += 1;
                hunger -= healthRegenerationHungerCost;
            }
        }
        else if (hunger > 17)
        {
            if (Time.time % 4f - Time.deltaTime <= 0)
            {
                health += 1;
                hunger -= healthRegenerationHungerCost;
            }
        }
    }

    [Server]
    private void CheckHunger()
    {
        if (Time.time % 1f - Time.deltaTime <= 0)
        {
            if (GetVelocity().x > 0.2f || GetVelocity().x < -0.2f)
                hunger -= movementHungerCost;
            if (sprinting)
                hunger -= sprintHungerCost;
            if (GetVelocity().y > 0)
                hunger -= jumpHungerCost;
        }
    }

    [Client]
    private void CheckActionBarUpdate()
    {
        Material selectedMaterial = GetInventory().GetSelectedItem().material;

        if (selectedMaterial != actionBarLastSelectedMaterial && selectedMaterial != Material.Air)
            ActionBar.message = selectedMaterial.ToString().Replace('_', ' ');

        actionBarLastSelectedMaterial = selectedMaterial;
    }

    [Server]
    private void CheckStarvationDamage()
    {
        if (hunger <= 0)
            if (Time.time % 4f - Time.deltaTime <= 0)
                TakeStarvationDamage(1);
    }

    [Server]
    public virtual void TakeStarvationDamage(float damage)
    {
        Damage(damage);
    }

    [Client]
    private void PerformInput()
    {
        if (ChatMenu.instance.open || SignEditMenu.IsLocalMenuOpen())
            return;

        //Open inventory
        if (Input.GetKeyDown(KeyCode.E) && framesSinceInventoryOpen > 10)
            RequestOpenInventory();

        if (Inventory.IsAnyOpen(playerInstance.GetComponent<PlayerInstance>()))
            return;

        //Open chat
        if (Input.GetKeyDown(KeyCode.T))
            ChatMenu.instance.open = true;

        //Walking
        if (Input.GetKey(KeyCode.A))
            Walk(-1);
        if (Input.GetKey(KeyCode.D))
            Walk(1);

        //Jumping
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space))
            Jump();

        //Sneaking
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.S))
        {
            SetServerSneaking(true);
            speed = sneakSpeed;
        }

        if (sneaking && (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.S)))
        {
            SetServerSneaking(false);
            speed = walkSpeed;
        }

        //Sprinting
        if (sprinting && 
            (Input.GetKeyUp(KeyCode.LeftControl) || Mathf.Abs(GetVelocity().x) < 0.5f || sneaking || hunger <= 6))
        {
            SetServerSprinting(false);
            speed = walkSpeed;
        }
        
        if (Input.GetKeyDown(KeyCode.LeftControl) && hunger > 6)
        {
            SetServerSprinting(true);
            speed = sprintSpeed;
        }

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
        {
            if (Time.time - lastDoubleTapSprintTap < 0.3f)
            {
                SetServerSprinting(true);
                speed = sprintSpeed;
            }
            
            lastDoubleTapSprintTap = Time.time;
        }

        //Debug disable lighting
        if (Debug.isDebugBuild && Input.GetKeyDown(KeyCode.F4))
            LightManager.instance.doLight = !LightManager.instance.doLight;

        //Inventory Managment
        if (Input.GetKeyDown(KeyCode.Q))
            RequestDropItem();

        KeyCode[] numpadCodes =
        {
            KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6
            , KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9
        };
        foreach (KeyCode keyCode in numpadCodes)
            if (Input.GetKeyDown(keyCode))
                SetSelectedInventorySlot(Array.IndexOf(numpadCodes, keyCode));


        float scroll = Input.mouseScrollDelta.y;
        //Check once every 5 frames
        if (scroll != 0 && (Time.frameCount % 5 == 0 || lastFrameScroll == 0))
        {
            int newSelectedSlot = GetInventory().selectedSlot + (scroll > 0 ? -1 : 1);
            if (newSelectedSlot > 8)
                newSelectedSlot = 0;
            if (newSelectedSlot < 0)
                newSelectedSlot = 8;

            SetSelectedInventorySlot(newSelectedSlot);
        }

        lastFrameScroll = scroll;

        MouseInput();
    }

    [Command]
    private void RequestOpenInventory()
    {
        GetInventory().Open(playerInstance.GetComponent<PlayerInstance>());
    }

    [Command]
    private void RequestDropItem()
    {
        DropSelected();
    }

    [Command]
    private void SetServerSprinting(bool sprint)
    {
        sprinting = sprint;
    }

    [Command]
    private void SetSelectedInventorySlot(int slot)
    {
        GetInventory().selectedSlot = slot;
    }

    [Command]
    private void SetServerSneaking(bool sneak)
    {
        sneaking = sneak;
    }

    public PlayerInventory GetInventory()
    {
        return (PlayerInventory) Inventory.Get(inventoryId);
    }

    [Client]
    public Location GetBlockedMouseLocation()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Location blockedMouseLocation = Location.LocationByPosition(mousePosition);

        return blockedMouseLocation;
    }

    [Client]
    public Entity GetMouseEntity()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D[] rays = Physics2D.RaycastAll(mousePosition, Vector2.zero);

        foreach (RaycastHit2D ray in rays)
            if (ray.collider != null && ray.transform.GetComponent<Entity>() != null)
                return ray.transform.GetComponent<Entity>();

        return null;
    }

    [Client]
    private void MouseInput()
    {
        if (!IsChunkLoaded())
            return;

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        bool isInRange = Vector2.Distance(mousePosition, transform.position) <= reach;

        UpdateCrosshair();

        EatItemInput();

        if (!isInRange)
            return;

        InteractEntityInput();
        BlockInteractionInput();
        BlockPlaceInput();
    }

    [Client]
    private void UpdateCrosshair()
    {
        if (crosshair == null)
        {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/Crosshair");
            crosshair = Instantiate(prefab);
        }

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        bool isInRange = Vector2.Distance(mousePosition, transform.position) <= reach;
        Entity entity = GetMouseEntity();

        string spriteName = "empty";
        if (isInRange)
        {
            if (entity == null)
                spriteName = "full";
            else
                spriteName = "entity";
        }

        crosshair.transform.position = GetBlockedMouseLocation().GetPosition();
        crosshair.GetComponent<SpriteRenderer>().sprite =
            Resources.Load<Sprite>("Sprites/crosshair_" + spriteName);
    }

    [Client]
    private void EatItemInput()
    {
        if (Input.GetMouseButtonUp(1))
        {
            RequestResetEatTime();
            return;
        }

        if (Input.GetMouseButton(1) && hunger <= maxHunger - 1 && Time.time % 0.2f - Time.deltaTime <= 0 &&
            Type.GetType(GetInventory().GetSelectedItem().material.ToString()).IsSubclassOf(typeof(Food)))
            RequestEat();
    }

    [Command]
    public void RequestEat()
    {
        if (!Type.GetType(GetInventory().GetSelectedItem().material.ToString()).IsSubclassOf(typeof(Food)))
            return;

        if (eatingTime > 1.3f)
        {
            EatHeldItem();
            eatingTime = 0;
            return;
        }

        Sound.Play(Location, "entity/Player/eat", SoundType.Entities, 0.85f, 1.15f);

        eatingTime += 0.2f;
    }

    [Command]
    public void RequestResetEatTime()
    {
        eatingTime = 0;
    }

    [Server]
    private void EatHeldItem()
    {
        ItemStack selectedItemStack = GetInventory().GetSelectedItem();

        //Add hunger
        Food foodItemType =
            (Food) Activator.CreateInstance(Type.GetType(GetInventory().GetSelectedItem().material.ToString()));
        int foodPoints = foodItemType.food_points;
        hunger = Mathf.Clamp(hunger + foodPoints, 0, maxHunger);

        //Particle Effect
        PlayEatEffect(selectedItemStack.GetTextureColors());

        //Burp sounds
        Sound.Play(Location, "entity/Player/burp", SoundType.Entities, 0.85f, 1.15f);

        //Subtract food item from inventory
        selectedItemStack.Amount -= 1;
        GetInventory().SetItem(GetInventory().selectedSlot, selectedItemStack);
    }

    [Client]
    private void InteractEntityInput()
    {
        //Hit Entities
        if (Input.GetMouseButtonDown(0))
        {
            Entity entity = GetMouseEntity();

            if (entity != null && entity != this)
                RequestHitEntity(entity.gameObject);
        }
        
        //Interract Entities
        if (Input.GetMouseButtonDown(1))
        {
            Entity entity = GetMouseEntity();

            if (entity != null && entity != this)
                RequestInteractEntity(entity.gameObject);
        }
    }
    
    [Client]
    private void BlockPlaceInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Location loc = GetBlockedMouseLocation();
            Material currentMaterial = loc.GetMaterial();

            if (GetMouseEntity() == null && (currentMaterial == Material.Air || currentMaterial == Material.Water ||
                                             currentMaterial == Material.Lava))
                RequestBlockPlace(loc);
        }
    }

    [Command]
    public void RequestBlockPlace(Location loc)
    {
        ItemStack item = GetInventory().GetSelectedItem();
        Material heldMat;

        if (GetInventory().GetSelectedItem().material == Material.Air || GetInventory().GetSelectedItem().Amount <= 0)
            return;

        if (Type.GetType(item.material.ToString()).IsSubclassOf(typeof(Block)))
            heldMat = item.material;
        else if (Type.GetType(item.material.ToString()).IsSubclassOf(typeof(PlaceableItem)))
            heldMat = ((PlaceableItem) Activator.CreateInstance(Type.GetType(item.material.ToString()))).blockMaterial;
        else
            return;

        loc.SetMaterial(heldMat);
        loc.GetBlock().BuildTick();
        loc.Tick();

        GetInventory().SetItem(GetInventory().selectedSlot, new ItemStack(item.material, item.Amount - 1));
        lastHitTime = NetworkTime.time;
    }

    [Client]
    private void BlockInteractionInput()
    {
        if (Time.time - lastBlockInteractionTime >= 1f / interactionsPerPerSecond)
        {
            if (Input.GetMouseButtonDown(1))
            {
                RequestInteract(GetBlockedMouseLocation(), 1, true);
                lastBlockInteractionTime = Time.time;
            }
            else if (Input.GetMouseButton(1))
            {
                RequestInteract(GetBlockedMouseLocation(), 1, false);
                lastBlockInteractionTime = Time.time;
            }

            if (Input.GetMouseButtonDown(0))
            {
                RequestInteract(GetBlockedMouseLocation(), 0, true);
                lastBlockInteractionTime = Time.time;
            }
            else if (Input.GetMouseButton(0))
            {
                RequestInteract(GetBlockedMouseLocation(), 0, false);
                lastBlockInteractionTime = Time.time;
            }
        }
    }

    [Command]
    public void RequestInteract(Location loc, int mouseButton, bool firstFrameDown, NetworkConnectionToClient sender = null)
    {
        //if the selected item derives from "Item", create in instance of item, else create empty
        //"Item", without any subclasses
        Item itemType;
        if (Type.GetType(GetInventory().GetSelectedItem().material.ToString()).IsSubclassOf(typeof(Item)))
            itemType = (Item) Activator.CreateInstance(
                Type.GetType(GetInventory().GetSelectedItem().material.ToString()));
        else
            itemType = (Item) Activator.CreateInstance(typeof(Item));

        PlayerInstance player = sender.identity.GetComponent<PlayerInstance>();

        itemType.Interact(player, loc, mouseButton, firstFrameDown);
        if (loc.GetMaterial() != Material.Air)
            lastBlockHitTime = NetworkTime.time;
    }

    [Server]
    public void DoToolDurability()
    {
        if (GetInventory().GetSelectedItem().GetMaxDurability() != -1)
        {
            ItemStack newItem = GetInventory().GetSelectedItem();
            newItem.durability--;

            GetInventory().SetItem(GetInventory().selectedSlot, (newItem.durability >= 0) ? newItem : new ItemStack());
        }
    }

    [Command]
    public virtual void RequestHitEntity(GameObject entityObj)
    {
        Entity entity = entityObj.GetComponent<Entity>();
        bool criticalHit = false;
        float damage = GetInventory().GetSelectedItem().GetItemEntityDamage();


        if (GetVelocity().y < -0.5f)
            criticalHit = true;

        if (criticalHit)
        {
            damage *= 1.5f;
            entity.CriticalDamageEffect();
        }

        if (GetInventory().GetSelectedItem().durability != -1)
            DoToolDurability();

        entity.transform.GetComponent<Entity>().Hit(damage, this);
        lastHitTime = NetworkTime.time;
    }
    
    [Command]
    public virtual void RequestInteractEntity(GameObject entityObj)
    {
        Entity entity = entityObj.GetComponent<Entity>();

        entity.transform.GetComponent<Entity>().Interact(this);
        lastHitTime = NetworkTime.time;
    }

    [Server]
    public override List<ItemStack> GetDrops()
    {
        List<ItemStack> result = new List<ItemStack>();

        result.AddRange(GetInventory().items);

        return result;
    }

    [Server]
    public void DropSelected()
    {
        ItemStack selectedItem = GetInventory().GetSelectedItem();
        ItemStack droppedItem = selectedItem;
        
        if (droppedItem.Amount <= 0)
            return;

        droppedItem.Amount = 1;
        selectedItem.Amount--;
        DropItem(droppedItem);
        GetInventory().SetItem(GetInventory().selectedSlot, selectedItem);
    }
    
    [Server]
    public void DropItem(ItemStack item)
    {
        ItemStack droppedItem = item;
        
        droppedItem.Drop(Location + new Location(1 * (facingLeft ? -1 : 1), 0)
            , new Vector2(3 * (facingLeft ? -1 : 1), 0));
    }

    [Server]
    public override void DropAllDrops()
    {
        base.DropAllDrops();

        GetInventory().Clear();
    }

    [Server]
    public Location ValidSpawn(int x)
    {
        Block topmostBlock = Chunk.GetTopmostBlock(x, Location.dimension, true);

        if (topmostBlock == null)
            return new Location(x, 80, Location.dimension);

        return topmostBlock.location + new Location(0, 2);
    }

    [Server]
    public override void Save()
    {
        if (!Directory.Exists(WorldManager.world.GetPath() + "\\players\\" + uuid))
            Directory.CreateDirectory(WorldManager.world.GetPath() + "\\players\\" + uuid);

        base.Save();
        PlayerSaveData.SetBedLocation(uuid, bedLocation);
    }

    [Server]
    public override void Load()
    {
        base.Load();
        bedLocation = PlayerSaveData.GetBedLocation(uuid);
    }

    [Server]
    public void Sleep()
    {
        int currentDay = (int) (WorldManager.instance.worldTime / WorldManager.DayLength);
        float newTime = (currentDay + 1) * WorldManager.DayLength;
        bool isNight = WorldManager.instance.worldTime % WorldManager.DayLength > WorldManager.DayLength / 2;

        if (isNight)
            WorldManager.world.time = newTime;
        if (WorldManager.world.weather != Weather.Clear)
        {
            WeatherManager.instance.ChangeWeather();
        }
    }

    [Server]
    public override string SavePath()
    {
        return WorldManager.world.GetPath() + "\\players\\" + uuid + "\\entity.dat";
    }

    [Server]
    private IEnumerator ValidSpawnOnceChunkLoaded()
    {
        yield return new WaitForSeconds(0.2f);
        while (!IsChunkLoaded())
            yield return new WaitForSeconds(0.1f);

        Location validLoc = ValidSpawn(Location.x);

        Teleport(validLoc);
    }

    [Server]
    public override void Die()
    {
        if (dead)
            return;

        base.Die();
        File.Delete(SavePath());
        GetInventory().Delete();
        DeathMenuEffect();
        Destroy(crosshair);
    }

    [ClientRpc]
    public override void Knockback(Vector2 direction)
    {
        if (hasAuthority)
            ClientKnockback(direction);
    }

    public void ClientKnockback(Vector2 direction)
    {
        base.Knockback(direction);
    }

    [ClientRpc]
    public void DeathMenuEffect()
    {
        if (hasAuthority)
            DeathMenu.active = true;
    }

    [Server]
    private void ClimbableSound()
    {
        if (isOnClimbable && Mathf.Abs(GetVelocity().y) > 0.5f)
            if (Time.time % 0.8f - Time.deltaTime <= 0)
                Sound.Play(Location, "block/ladder/hit", SoundType.Entities, 0.8f, 1.2f);
    }

    [Client]
    private void CheckDimensionChangeLoadingScreen()
    {
        if (teleportingDimension)
            LoadingMenu.Create(LoadingMenuType.Dimension);
    }

    [Server]
    public override void Damage(float damage)
    {
        base.Damage(damage);

        PlayClientCameraShakeEffect();
    }

    [ClientRpc]
    public void PlayClientCameraShakeEffect()
    {
        if (hasAuthority)
            CameraController.instance.shake = 5;
    }

    [ClientRpc]
    public void PlayEatEffect(Color[] colors)
    {
        Random r = new Random();
        for (int i = 0; i < r.Next(6, 10); i++) //SpawnParticles
        {
            Particle part = Particle.Spawn();
            Color color = colors[r.Next(0, colors.Length)];
            part.transform.position = Location.GetPosition() + new Vector2(0, 0.2f);
            part.color = color;
            part.doGravity = true;
            part.velocity = new Vector2((0.5f + (float) r.NextDouble()) * (r.Next(0, 2) == 0 ? -1 : 1),
                3f + (float) r.NextDouble());
            part.maxAge = (float) r.NextDouble();
            part.maxBounces = 10;
        }
    }


    private void CalculateFlip()
    {
        if (Mathf.Abs(GetVelocity().x) > 0.1f)
            facingLeft = GetVelocity().x < 0;
    }
    
    [Client]
    public override void UpdateAnimatorValues()
    {
        base.UpdateAnimatorValues();

        Animator anim = GetComponent<Animator>();

        anim.SetBool("eating", eatingTime > 0);
        anim.SetBool("punch",
            NetworkTime.time - lastHitTime < 0.05f || NetworkTime.time - lastBlockHitTime < 0.3f);
        anim.SetBool("holding-item", GetInventory().GetSelectedItem().material != Material.Air);
        anim.SetBool("sneaking", sneaking);
    }
}