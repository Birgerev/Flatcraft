using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : HumanEntity
{
    public static float interactionsPerPerSecond = 4.5f;

    //Entity State
    [Space] public static Player localInstance;

    public GameObject crosshair;
    private int framesSinceInventoryOpen;
    private float eatingTime;
    private float lastBlockInteractionTime;
    private float lastFrameScroll;


    //Entity Data Tags
    [EntityDataTag(false)] public float hunger;
    [EntityDataTag(true)] public PlayerInventory inventory = new PlayerInventory();
    [EntityDataTag(true)] public Location bedLocation = new Location(0, 0);

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
        localInstance = this;

        hunger = maxHunger;
        inventory = new PlayerInventory();
        Cursor.visible = false;

        StartCoroutine(ValidSpawnOnceChunkLoaded());

        base.Start();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        RenderSpriteParts();
        performMovementInput();
    }

    public override void UpdateAnimatorValues()
    {
        base.UpdateAnimatorValues();

        var anim = GetComponent<Animator>();

        anim.SetBool("eating", ((Input.GetMouseButton(1) || Input.GetMouseButtonDown(1)) && Type.GetType(inventory.getSelectedItem().material.ToString()).IsSubclassOf(typeof(Food))) && hunger <= maxHunger - 1);
        anim.SetBool("punch", Input.GetMouseButton(0) || Input.GetMouseButtonDown(1));
        anim.SetBool("holding-item", inventory.getSelectedItem().material != Material.Air);
        anim.SetBool("sneaking", sneaking);
        anim.SetBool("grounded", isOnGround);

        GetRenderer().transform.localScale = new Vector2(facingLeft ? -1 : 1, 1); //Mirror renderer if facing left
    }

    private void RenderSpriteParts()
    {
        for (var i = 0; i < GetRenderer().transform.childCount; i++)
        {
            var spritePart = GetRenderer().transform.GetChild(i).GetComponent<SpriteRenderer>();
            spritePart.color = GetRenderer().color;
        }
    }


    public override void Update()
    {
        base.Update();

        var scroll = Input.mouseScrollDelta.y;
        //Check once every 5 frames
        if (scroll != 0 && (Time.frameCount % 5 == 0 || lastFrameScroll == 0))
        {
            inventory.selectedSlot += scroll > 0 ? -1 : 1;

            if (inventory.selectedSlot > 8)
                inventory.selectedSlot = 0;
            if (inventory.selectedSlot < 0)
                inventory.selectedSlot = 8;
        }

        lastFrameScroll = scroll;

        if (InventoryMenuManager.instance.anyInventoryOpen())
            framesSinceInventoryOpen = 0;
        else
            framesSinceInventoryOpen++;

        CheckHunger();
        CheckRegenerateHealth();
        CheckStarvationDamage();

        //Crosshair
        MouseInput();
        PerformInput();
    }

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

    private void CheckStarvationDamage()
    {
        if (hunger <= 0)
            if ((Time.time % 4f) - Time.deltaTime <= 0)
                TakeStarvationDamage(1);
    }

    public virtual void TakeStarvationDamage(float damage)
    {
        Damage(damage);
    }


    private void performMovementInput()
    {
        if (InventoryMenuManager.instance.anyInventoryOpen())
            return;

        //Movement
        if (Input.GetKey(KeyCode.A)) Walk(-1);
        if (Input.GetKey(KeyCode.D)) Walk(1);
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space)) Jump();
    }

    private void PerformInput()
    {
        if (InventoryMenuManager.instance.anyInventoryOpen())
            return;

        if (Input.GetKeyDown(KeyCode.E) && framesSinceInventoryOpen > 10)
            inventory.Open(Location);

        if (Inventory.anyOpen)
            return;

        sneaking = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.S);

        if (Input.GetKey(KeyCode.LeftControl) && hunger > 6) 
            sprinting = true;
        if (Mathf.Abs(GetVelocity().x) < 3f || sneaking || hunger <= 6) 
            sprinting = false;


        //Inventory Managment
        if (Input.GetKeyDown(KeyCode.Q))
            DropSelected();

        KeyCode[] numpadCodes =
        {
            KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6,
            KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9
        };
        foreach (var keyCode in numpadCodes)
            if (Input.GetKeyDown(keyCode))
                inventory.selectedSlot = Array.IndexOf(numpadCodes, keyCode);
    }
    
    public Location GetBlockedMouseLocation()
    {
        var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var blockedMouseLocation = Location.LocationByPosition(mousePosition, Location.dimension);
        
        return blockedMouseLocation;
    }

    public Block GetMouseBlock()
    {
        Location blockedMouseLoc = GetBlockedMouseLocation();

        if (blockedMouseLoc.y < 0 || blockedMouseLoc.y > Chunk.Height)
            return null;

        return blockedMouseLoc.GetBlock();
    }

    public Entity GetMouseEntity()
    {
        var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var hitEntity = Physics2D.Raycast(mousePosition, Vector2.zero);

        if (hitEntity.collider == null || hitEntity.transform.GetComponent<Entity>() == null)
            return null;

        return hitEntity.transform.GetComponent<Entity>();
    }

    private void MouseInput()
    {
        if (WorldManager.instance.loadingProgress != 1)
            return;
        if (InventoryMenuManager.instance.anyInventoryOpen())
            return;

        var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        var isInRange = Mathf.Abs((mousePosition - transform.position).magnitude) <= reach;

        var entity = GetMouseEntity();
        var block = GetMouseBlock();

        crosshair.transform.position = GetBlockedMouseLocation().GetPosition();
        crosshair.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/crosshair_" + (isInRange ? (entity != null ? "entity" : "full") : "empty"));

        //Eating
        if (Input.GetMouseButton(1) && hunger <= maxHunger - 1 && Type.GetType(inventory.getSelectedItem().material.ToString()).IsSubclassOf(typeof(Food)))
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


        if (!isInRange)
            return;

        //Hit Entities
        if (Input.GetMouseButtonDown(0))
        {
            if (entity != null)
            {
                HitEntity(entity.transform.GetComponent<Entity>());
                return;
            }

        }

        //Place blocks
        if (Input.GetMouseButtonDown(1))
        {
            if (block == null || block.GetMaterial() == Material.Water || block.GetMaterial() == Material.Lava)
            {
                ItemStack item = inventory.getSelectedItem().Clone();
                Material mat;

                if (inventory.getSelectedItem().material == Material.Air || inventory.getSelectedItem().amount <= 0)
                    return;

                if (Type.GetType(item.material.ToString()).IsSubclassOf(typeof(Block)))
                {
                    mat = item.material;
                }
                else if (Type.GetType(item.material.ToString()).IsSubclassOf(typeof(PlaceableItem)))
                {
                    mat = ((PlaceableItem)Activator.CreateInstance(Type.GetType(item.material.ToString()))).blockMaterial;
                }
                else return;

                GetBlockedMouseLocation().SetMaterial(mat);
                GetBlockedMouseLocation().GetBlock().ScheduleBlockBuildTick();
                GetBlockedMouseLocation().Tick();

                inventory.setItem(inventory.selectedSlot, new ItemStack(item.material, item.amount - 1));
                return;
            }
        }

        if (Time.time - lastBlockInteractionTime >= 1f / interactionsPerPerSecond)
        {
            Item itemType; //if the selected item derives from "Item", create in instance of item, else create basic "Item", without any subclasses
            if (Type.GetType(inventory.getSelectedItem().material.ToString()).IsSubclassOf(typeof(Item)))
                itemType = (Item)Activator.CreateInstance(Type.GetType(inventory.getSelectedItem().material.ToString()));
            else
                itemType = (Item)Activator.CreateInstance(typeof(Item));


            if (Input.GetMouseButtonDown(1))
            {
                itemType.Interact(GetBlockedMouseLocation(), 1, true);
                lastBlockInteractionTime = Time.time;
            }
            else if (Input.GetMouseButton(1))
            {
                itemType.Interact(GetBlockedMouseLocation(), 1, false);
                lastBlockInteractionTime = Time.time;
            }

            if (Input.GetMouseButtonDown(0))
            {
                itemType.Interact(GetBlockedMouseLocation(), 0, true);
                lastBlockInteractionTime = Time.time;
            }
            else if (Input.GetMouseButton(0))
            {
                itemType.Interact(GetBlockedMouseLocation(), 0, false);
                lastBlockInteractionTime = Time.time;
            }
        }
    }

    public void DoToolDurability()
    {
        if (inventory.getSelectedItem().GetMaxDurability() != -1)
        {
            inventory.getSelectedItem().durability--;

            if (inventory.getSelectedItem().durability < 0)
                inventory.setItem(inventory.selectedSlot, new ItemStack());
        }
    }

    private void EatHeldItem()
    {
        //Subtract food item from inventory
        ItemStack selectedItemStack = inventory.getSelectedItem();
        selectedItemStack.amount -= 1;
        inventory.setItem(inventory.selectedSlot, selectedItemStack);

        //Add hunger
        Food foodItemType = (Food)Activator.CreateInstance(Type.GetType(inventory.getSelectedItem().material.ToString()));
        int foodPoints = foodItemType.food_points;
        hunger = Mathf.Clamp(hunger + foodPoints, 0, maxHunger);

        //Particle Effect
        PlayEatEffect(selectedItemStack.GetTextureColors());

        //Burp sounds
        Sound.Play(Location, "entity/Player/burp", SoundType.Entities, 0.85f, 1.15f);
    }

    public void PlayEatEffect(Color[] colors)
    {
        var r = new System.Random();
        for (var i = 0; i < r.Next(6, 10); i++) //SpawnParticles
        {
            var particle = (Particle)Entity.Spawn("Particle");
            Color color = colors[r.Next(0, colors.Length)];
            particle.transform.position = Location.GetPosition() + new Vector2(0, 0.2f);
            particle.color = color;
            particle.doGravity = true;
            particle.velocity = new Vector2(
                (0.5f + (float)r.NextDouble()) * (r.Next(0, 2) == 0 ? -1 : 1), 
                3f + (float)r.NextDouble());
            particle.maxAge = (float)r.NextDouble();
            particle.maxBounces = 10;
        }
    }

    public virtual void HitEntity(Entity entity)
    {
        bool criticalHit = false;
        float damage = inventory.getSelectedItem().GetItemEntityDamage();

        
        if (GetVelocity().y < -0.5f)
            criticalHit = true;

        if (criticalHit)
        {
            damage *= 1.5f;
            entity.PlayCriticalDamageEffect();
        }

        if (inventory.getSelectedItem().durability != -1)
            DoToolDurability();

        entity.transform.GetComponent<Entity>().Hit(damage);
    }

    public override List<ItemStack> GetDrops()
    {
        var result = new List<ItemStack>();

        result.AddRange(inventory.items);

        return result;
    }

    public void DropSelected()
    {
        var item = inventory.getSelectedItem().Clone();

        if (item.amount <= 0)
            return;

        item.amount = 1;
        inventory.getSelectedItem().amount--;

        item.Drop(Location + new Location(1 * (facingLeft ? -1 : 1), 0), new Vector2(3 * (facingLeft ? -1 : 1), 0));
    }

    public override void DropAllDrops()
    {
        base.DropAllDrops();

        inventory.Clear();
    }

    public Location ValidSpawn(int x)
    {
        var topmostBlock = Chunk.GetTopmostBlock(x, Location.dimension, true);

        if (topmostBlock == null) return new Location(x, 80, Location.dimension);

        return topmostBlock.location + new Location(0, 2);
    }

    public override void Die()
    {
        DeathMenu.active = true;
        health = 20;
        hunger = 20;
        fireTime = 0;

        base.Die();

        Location spawnLocation = new Location(0, 80, Dimension.Overworld);

        if (bedLocation.GetMaterial() == Material.Bed_Bottom || bedLocation.GetMaterial() == Material.Bed_Top)
            spawnLocation = bedLocation;

        Location = spawnLocation;
        UpdateCachedPosition();
        Save();
    }

    public override void Damage(float damage)
    {
        base.Damage(damage);

        Sound.Play(Location, "entity/Player/hurt", SoundType.Entities, 0.85f, 1.15f); //Play hurt sound
    }

    public override void Hit(float damage)
    {
    }

    public void Sleep()
    {
        var currentDay = (int) (WorldManager.world.time / WorldManager.dayLength);
        var newTime = (currentDay + 1) * WorldManager.dayLength;
        var isNight = WorldManager.world.time % WorldManager.dayLength > WorldManager.dayLength / 2;

        if (isNight) WorldManager.world.time = newTime;
    }

    public override string SavePath()
    {
        return WorldManager.world.getPath() + "\\players\\player.dat";
    }

    private Dictionary<string, string> dataFromString(string[] lines)
    {
        var resultData = new Dictionary<string, string>();

        foreach (var line in lines)
            if (line.Contains("="))
                resultData.Add(line.Split('=')[0], line.Split('=')[1]);

        return resultData;
    }

    private IEnumerator ValidSpawnOnceChunkLoaded()
    {
        yield return new WaitForSeconds(0.2f);
        while (!IsChunkLoaded()) 
            yield return new WaitForSeconds(0.1f);

        highestYlevelsinceground = 0; //Reset falldamage
        Location validLoc = ValidSpawn(Location.x);

        Location = validLoc;
    }
}