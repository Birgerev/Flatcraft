using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Mirror;
using UnityEngine;

public class Player : HumanEntity
{
    //TODO sync sound
    //TODO time of day sync
    //TODO quit game
    //TODO singleplayer
    //TODO wipe inventory on death
    //TODO punch animation shouldn't be local
    //TODO block interaction
    //TODO block breaking indicator
    //TODO block breaking
    //TODO eating
    //TODO particles dont appear on client (run particles)
    public static float interactionsPerPerSecond = 4.5f;

    //Entity State
    public static Player localEntity;
    public static List<Player> players = new List<Player>();

    [SyncVar] public GameObject playerInstance;
    public GameObject crosshair;
    private int framesSinceInventoryOpen;
    private float eatingTime;
    private float lastBlockInteractionTime;
    private float lastFrameScroll;


    //Entity Data Tags
    [EntityDataTag(false)] [SyncVar]
    public float hunger;
    [EntityDataTag(true)] [SyncVar]
    public int inventoryId;
    public Location bedLocation = new Location(0, 0);

    //Entity Properties
    public override bool ChunkLoadingEntity { get; } = true;
    public override float maxHealth { get; } = 20;
    public float maxHunger = 20;
    public float reach = 5;
    private float movementHungerCost = 0.03f;
    private float sprintHungerCost = 0.03f;
    private float jumpHungerCost = 0.1f;
    private float healthRegenerationHungerCost = 0.4f;
    
    public override void Start()
    {
        players.Add(this);
        
        base.Start();
    }

    [Client]
    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        localEntity = this;
        gameObject.AddComponent<AudioListener>();
    }

    [Server]
    public override void Spawn()
    {
        base.Spawn();
        
        hunger = maxHunger;
        Inventory inv = PlayerInventory.CreatePreset();
        inventoryId = inv.id;
        
        if (bedLocation.GetMaterial() == Material.Bed_Bottom || bedLocation.GetMaterial() == Material.Bed_Top)
            Teleport(bedLocation);
        
        StartCoroutine(ValidSpawnOnceChunkLoaded());
    }

    [Server]
    public override void Tick()
    {
        base.Tick();
        
        CheckHunger();
        CheckRegenerateHealth();
        CheckStarvationDamage();
        ClimbableSound();
    }

    [Client]
    public override void ClientUpdate()
    {
        //Crosshair
        if (hasAuthority)
        {
            PerformInput();
            
            if(!isServer)     //If we are server, Process movement will already have been called in LivingEntity.Tick()
                ProcessMovement();
        
            if (GetInventory().open)
                framesSinceInventoryOpen = 0;
            else
                framesSinceInventoryOpen++;
        }
        
        RenderSpriteParts();
        
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
        if(hasAuthority)
            base.ProcessMovement();
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

        if(hunger > 19)
        {
            if ((Time.time % 0.5f) - Time.deltaTime <= 0)
            {
                health += 1;
                hunger -= healthRegenerationHungerCost;
            }
        }
        else if (hunger > 17)
        {
            if ((Time.time % 4f) - Time.deltaTime <= 0)
            {
                health += 1;
                hunger -= healthRegenerationHungerCost;
            }
        }
    }

    [Server]
    private void CheckHunger()
    {
        if ((Time.time % 1f) - Time.deltaTime <= 0)
        {
            if (GetVelocity().x > 0.2f || GetVelocity().x < -0.2f)
                hunger -= movementHungerCost;
            if (sprinting)
                hunger -= sprintHungerCost;
            if (GetVelocity().y > 0)
                hunger -= jumpHungerCost;
        }
    }

    [Server]
    private void CheckStarvationDamage()
    {
        if (hunger <= 0)
            if ((Time.time % 4f) - Time.deltaTime <= 0)
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
        if (Input.GetKeyDown(KeyCode.E) && framesSinceInventoryOpen > 10)
            RequestOpenInventory();

        if (GetInventory().open)
            return;
        
        if (Input.GetKey(KeyCode.A)) 
            Walk(-1);
        if (Input.GetKey(KeyCode.D)) 
            Walk(1);
        
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space)) 
            Jump();

        if((Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.S)))
            SetSneaking(false);
        if((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.S)) && !sneaking)
            SetSneaking(true);
        
        
        if(Input.GetKeyDown(KeyCode.LeftControl) && hunger > 6)
            SetSprinting(true);
        if(Input.GetKeyUp(KeyCode.LeftControl) || Mathf.Abs(GetVelocity().x) < 3f || sneaking || hunger <= 6)
            SetSprinting(false);

        if (Input.GetKeyDown(KeyCode.F4))
            LightManager.instance.doLight = !LightManager.instance.doLight;

        //Inventory Managment
        if (Input.GetKeyDown(KeyCode.Q))
            RequestDropItem();

        KeyCode[] numpadCodes =
        {
            KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6,
            KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9
        };
        foreach (var keyCode in numpadCodes)
            if (Input.GetKeyDown(keyCode))
                SetSelectedInventorySlot(Array.IndexOf(numpadCodes, keyCode));
        
        
        var scroll = Input.mouseScrollDelta.y;
        //Check once every 5 frames
        if (scroll != 0 && (Time.frameCount % 5 == 0 || lastFrameScroll == 0))
        {
            int newSelectedSlot = GetInventory().selectedSlot + (scroll > 0 ? -1 : 1);
            if (newSelectedSlot > 8) newSelectedSlot = 0;
            if (newSelectedSlot < 0) newSelectedSlot = 8;
            
            SetSelectedInventorySlot(newSelectedSlot);
        }
        lastFrameScroll = scroll;
        
        MouseInput();
    }
    
    [Command]
    private void RequestOpenInventory()
    {
        GetInventory().Open(playerInstance);
    }
    
    [Command]
    private void RequestDropItem()
    {
        DropSelected();
    }
    
    [Command]
    private void SetSprinting(bool sprint)
    {
        sprinting = sprint;
    }
    
    [Command]
    private void SetSelectedInventorySlot(int slot)
    {
        GetInventory().selectedSlot = slot;
    }
    
    [Command]
    private void SetSneaking(bool sneak)
    {
        sneaking = sneak;
    }

    public PlayerInventory GetInventory()
    {
        return (PlayerInventory)Inventory.Get(inventoryId);
    }
    
    [Client]
    public Location GetBlockedMouseLocation()
    {
        var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var blockedMouseLocation = Location.LocationByPosition(mousePosition, Location.dimension);
        
        return blockedMouseLocation;
    }

    [Client]
    public Block GetMouseBlock()
    {
        Location blockedMouseLoc = GetBlockedMouseLocation();

        if (blockedMouseLoc.y < 0 || blockedMouseLoc.y > Chunk.Height)
            return null;

        return blockedMouseLoc.GetBlock();
    }

    [Client]
    public Entity GetMouseEntity()
    {
        var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var hitEntity = Physics2D.Raycast(mousePosition, Vector2.zero);

        if (hitEntity.collider == null || hitEntity.transform.GetComponent<Entity>() == null)
            return null;

        return hitEntity.transform.GetComponent<Entity>();
    }

    [Client]
    private void MouseInput()
    {
        if (WorldManager.instance.loadingProgress != 1)
            return;
        
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        bool isInRange = (Vector2.Distance(mousePosition, transform.position) <= reach);
        
        UpdateCrosshair();
        
        //TODO
        //EatItemInput();

        if (!isInRange)
            return;

        HitEntityInput();
        BlockPlaceInput();
        BlockInteractionInput();
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
        bool isInRange = (Vector2.Distance(mousePosition, transform.position) <= reach);
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
        if (Input.GetMouseButton(1) && hunger <= maxHunger - 1 && Type.GetType(GetInventory().GetSelectedItem().material.ToString()).IsSubclassOf(typeof(Food)))
        {
            if(eatingTime > 1.3f)
            {
                EatHeldItem();
                eatingTime = 0;
                return;
            }

            if ((eatingTime % 0.2f) - Time.deltaTime <= 0)
            {
                System.Random r = new System.Random();

                Sound.Play(Location, "entity/Player/eat"+r.Next(1, 3), SoundType.Entities, 0.85f, 1.15f);
            }

            eatingTime += Time.deltaTime;
            return;
        }
        
        eatingTime = 0;
    }

    [Client]
    private void HitEntityInput()
    {
        //Hit Entities
        if (Input.GetMouseButtonDown(0))
        {
            Entity entity = GetMouseEntity();
            
            if (entity != null && entity != this)
            {
                RequestHitEntity(entity.gameObject);
            }
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
            {
                RequestBlockPlace(loc);
            }
        }
    }

    [Command]
    public void RequestBlockPlace(Location loc)
    {
        ItemStack item = GetInventory().GetSelectedItem().Clone();
        Material heldMat;
        
        if (GetInventory().GetSelectedItem().material == Material.Air || GetInventory().GetSelectedItem().amount <= 0)
            return;

        if (Type.GetType(item.material.ToString()).IsSubclassOf(typeof(Block)))
        {
            heldMat = item.material;
        }
        else if (Type.GetType(item.material.ToString()).IsSubclassOf(typeof(PlaceableItem)))
        {
            heldMat = ((PlaceableItem)Activator.CreateInstance(Type.GetType(item.material.ToString()))).blockMaterial;
        }
        else return;

        loc.SetMaterial(heldMat);
        //TODO
        //loc.GetBlock().ScheduleBlockBuildTick();
        loc.Tick();

        GetInventory().SetItem(GetInventory().selectedSlot, new ItemStack(item.material, item.amount - 1));
    }

    [Client]
    private void BlockInteractionInput()
    {
        //TODO
        if (Time.time - lastBlockInteractionTime >= 1f / interactionsPerPerSecond)
        {
            if (Input.GetMouseButtonDown(1))
            {
                RequestInteract(1, true);
                lastBlockInteractionTime = Time.time;
            }
            else if (Input.GetMouseButton(1))
            {
                RequestInteract(1, false);
                lastBlockInteractionTime = Time.time;
            }

            if (Input.GetMouseButtonDown(0))
            {
                RequestInteract(0, true);
                lastBlockInteractionTime = Time.time;
            }
            else if (Input.GetMouseButton(0))
            {
                RequestInteract(0, false);
                lastBlockInteractionTime = Time.time;
            }
        }
    }

    [Command]
    public void RequestInteract(int mouseButton, bool firstFrameDown)
    {
        //if the selected item derives from "Item", create in instance of item, else create empty
        //"Item", without any subclasses
        Item itemType; 
        if (Type.GetType(GetInventory().GetSelectedItem().material.ToString()).IsSubclassOf(typeof(Item)))
            itemType = (Item)Activator.CreateInstance(Type.GetType(GetInventory().GetSelectedItem().material.ToString()));
        else
            itemType = (Item)Activator.CreateInstance(typeof(Item));

        itemType.Interact(GetBlockedMouseLocation(), mouseButton, firstFrameDown);
    }
    
    [Server]
    public void DoToolDurability()
    {
        if (GetInventory().GetSelectedItem().GetMaxDurability() != -1)
        {
            GetInventory().GetSelectedItem().durability--;

            if (GetInventory().GetSelectedItem().durability < 0)
                GetInventory().SetItem(GetInventory().selectedSlot, new ItemStack());
        }
    }

    [Server]
    private void EatHeldItem()
    {
        ItemStack selectedItemStack = GetInventory().GetSelectedItem();

        //Add hunger
        Food foodItemType = (Food)Activator.CreateInstance(Type.GetType(GetInventory().GetSelectedItem().material.ToString()));
        int foodPoints = foodItemType.food_points;
        hunger = Mathf.Clamp(hunger + foodPoints, 0, maxHunger);

        //Particle Effect
        PlayEatEffect(selectedItemStack.GetTextureColors());

        //Burp sounds
        Sound.Play(Location, "entity/Player/burp", SoundType.Entities, 0.85f, 1.15f);
        
        //Subtract food item from inventory
        selectedItemStack.amount -= 1;
        GetInventory().SetItem(GetInventory().selectedSlot, selectedItemStack);
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
    }
    
    [Server]
    public override List<ItemStack> GetDrops()
    {
        var result = new List<ItemStack>();

        result.AddRange(GetInventory().items);

        return result;
    }

    [Server]
    public void DropSelected()
    {
        ItemStack selectedItem = GetInventory().GetSelectedItem().Clone();
        ItemStack droppedItem = selectedItem.Clone();
        droppedItem.amount = 1;
        selectedItem.amount --;

        if (droppedItem.amount <= 0)
            return;


        droppedItem.Drop(Location + new Location(1 * (facingLeft ? -1 : 1), 0), new Vector2(3 * (facingLeft ? -1 : 1), 0));
        GetInventory().SetItem(GetInventory().selectedSlot, selectedItem);
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
        var topmostBlock = Chunk.GetTopmostBlock(x, Location.dimension, true);

        if (topmostBlock == null) 
            return new Location(x, 80, Location.dimension);

        return topmostBlock.location + new Location(0, 2);
    }
    
    [Server]
    public override void Save()
    {
        if (!Directory.Exists(WorldManager.world.getPath() + "\\players\\"+displayName))
            Directory.CreateDirectory(WorldManager.world.getPath() + "\\players\\"+displayName);
        
        base.Save();
        PlayerSaveData.SetBedLocation(displayName, bedLocation);
    }
    
    [Server]
    public override void Load()
    {
        base.Load();
        bedLocation = PlayerSaveData.GetBedLocation(displayName);
    }
    
    [Server]
    public void Sleep()
    {
        var currentDay = (int) (WorldManager.world.time / WorldManager.dayLength);
        var newTime = (currentDay + 1) * WorldManager.dayLength;
        var isNight = WorldManager.world.time % WorldManager.dayLength > WorldManager.dayLength / 2;

        if (isNight) 
            WorldManager.world.time = newTime;
    }

    [Server]
    public override string SavePath()
    {
        return WorldManager.world.getPath() + "\\players\\"+displayName+"\\entity.dat";
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
        base.Die();
        DeathMenuEffect();
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
        if(hasAuthority)
            DeathMenu.active = true;
    }

    [Server]
    private void ClimbableSound()
    {
        if (isOnClimbable && Mathf.Abs(GetVelocity().y) > 0.5f)
            if ((Time.time % 0.8f) - Time.deltaTime <= 0)
                Sound.Play(Location, "block/ladder/hit", SoundType.Entities, 0.8f, 1.2f);
                
    }
    
    [ClientRpc]
    public void PlayEatEffect(Color[] colors)
    {
        var r = new System.Random();
        for (var i = 0; i < r.Next(6, 10); i++) //SpawnParticles
        {
            Particle part = Particle.Spawn();
            Color color = colors[r.Next(0, colors.Length)];
            part.transform.position = Location.GetPosition() + new Vector2(0, 0.2f);
            part.color = color;
            part.doGravity = true;
            part.velocity = new Vector2(
                (0.5f + (float)r.NextDouble()) * (r.Next(0, 2) == 0 ? -1 : 1), 
                3f + (float)r.NextDouble());
            part.maxAge = (float)r.NextDouble();
            part.maxBounces = 10;
        }
    }

    [Client]
    public override void UpdateAnimatorValues()
    {
        base.UpdateAnimatorValues();

        var anim = GetComponent<Animator>();

        anim.SetBool("eating", ((Input.GetMouseButton(1) || Input.GetMouseButtonDown(1)) && Type.GetType(GetInventory().GetSelectedItem().material.ToString()).IsSubclassOf(typeof(Food))) && hunger <= maxHunger - 1);
        anim.SetBool("punch", Input.GetMouseButton(0) || Input.GetMouseButtonDown(1));
        anim.SetBool("holding-item", GetInventory().GetSelectedItem().material != Material.Air);
        anim.SetBool("sneaking", sneaking);
        anim.SetBool("grounded", isOnGround);

        GetRenderer().transform.localScale = new Vector2(facingLeft ? -1 : 1, 1); //Mirror renderer if facing left
    }

    [Client]
    private void RenderSpriteParts()
    {
        for (var i = 0; i < GetRenderer().transform.childCount; i++)
        {
            var spritePart = GetRenderer().transform.GetChild(i).GetComponent<SpriteRenderer>();
            spritePart.color = GetRenderer().color;
        }
    }
}