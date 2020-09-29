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


    //Entity Data Tags
    [EntityDataTag(false)] public float hunger;

    [EntityDataTag(true)] public PlayerInventory inventory = new PlayerInventory();

    private float lastBlockInteractionTime;
    private float lastFrameScroll;
    public float maxHunger = 20;
    public float reach = 5;

    [EntityDataTag(true)] public Location spawnLocation = new Location(0, 80);

    //Entity Properties
    public override bool ChunkLoadingEntity { get; } = true;
    public override float maxHealth { get; } = 20;

    public override void Start()
    {
        localInstance = this;

        hunger = maxHunger;
        inventory = new PlayerInventory();

        ///StartCoroutine(MoveToValidSpawnOnceLoaded());

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

        //Crosshair
        mouseInput();
        performInput();
    }

    private void performMovementInput()
    {
        //Movement
        if (Input.GetKey(KeyCode.A)) Walk(-1);
        if (Input.GetKey(KeyCode.D)) Walk(1);
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space)) Jump();
    }

    private void performInput()
    {
        if (Input.GetKeyDown(KeyCode.E) && framesSinceInventoryOpen > 10)
            inventory.Open(Location);

        if (Inventory.anyOpen)
            return;

        sneaking = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.S);

        if (Input.GetKey(KeyCode.LeftControl)) sprinting = true;
        if (Mathf.Abs(GetVelocity().x) < 3f || sneaking) sprinting = false;


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

    private void mouseInput()
    {
        if (WorldManager.instance.loadingProgress != 1)
            return;

        var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var blockedMouseLocation = Location.LocationByPosition(mousePosition, Location.dimension);
        var mouseBlock = blockedMouseLocation.GetBlock();

        mousePosition.z = 0;
        var isInRange = Mathf.Abs((mousePosition - transform.position).magnitude) <= reach;
        var isAboveEntity = false;


        var hitEntity = Physics2D.Raycast(mousePosition, Vector2.zero);
        if (hitEntity.collider != null && hitEntity.transform.GetComponent<Entity>() != null) isAboveEntity = true;


        crosshair.transform.position = blockedMouseLocation.GetPosition();
        crosshair.GetComponent<SpriteRenderer>().sprite =
            Resources.Load<Sprite>("Sprites/crosshair_" + (isInRange ? isAboveEntity ? "entity" : "full" : "empty"));


        if (!isInRange)
            return;

        //Hit Entities
        if (isAboveEntity && Input.GetMouseButtonDown(0)) hitEntity.transform.GetComponent<Entity>().Hit(1);


        Item itemType;
        if (Type.GetType(inventory.getSelectedItem().material.ToString()).IsSubclassOf(typeof(Item))
        ) //if the selected item derives from "Item", create in instance of item, else create basic "Item", without any subclasses
            itemType = (Item) Activator.CreateInstance(Type.GetType(inventory.getSelectedItem().material.ToString()));
        else
            itemType = (Item) Activator.CreateInstance(typeof(Item));

        if (Type.GetType(inventory.getSelectedItem().material.ToString()).IsSubclassOf(typeof(Block)) || Type
            .GetType(inventory.getSelectedItem().material.ToString()).IsSubclassOf(typeof(PlaceableItem)))
            if (mouseBlock == null || mouseBlock.GetMaterial() == Material.Water ||
                mouseBlock.GetMaterial() == Material.Lava)
                if (Input.GetMouseButtonDown(1))
                    if (inventory.getSelectedItem().material != Material.Air &&
                        inventory.getSelectedItem().amount > 0)
                    {
                        if (Type.GetType(inventory.getSelectedItem().material.ToString())
                            .IsSubclassOf(typeof(PlaceableItem)))
                        {
                            blockedMouseLocation.SetMaterial(((PlaceableItem) itemType).blockMaterial);
                            blockedMouseLocation.Tick();
                        }

                        if (Type.GetType(inventory.getSelectedItem().material.ToString()).IsSubclassOf(typeof(Block)))
                        {
                            blockedMouseLocation.SetMaterial(inventory.getSelectedItem().material);
                            blockedMouseLocation.Tick();
                        }

                        inventory.setItem(inventory.selectedSlot,
                            new ItemStack(inventory.getSelectedItem().material,
                                inventory.getSelectedItem().amount - 1));

                        return;
                    }


        if (Time.time - lastBlockInteractionTime < 1f / interactionsPerPerSecond)
            return;

        if (Input.GetMouseButtonDown(1))
        {
            itemType.Interact(blockedMouseLocation, 1, true);
            lastBlockInteractionTime = Time.time;
        }
        else if (Input.GetMouseButton(1))
        {
            itemType.Interact(blockedMouseLocation, 1, false);
            lastBlockInteractionTime = Time.time;
        }

        if (Input.GetMouseButtonDown(0))
        {
            itemType.Interact(blockedMouseLocation, 0, true);
            lastBlockInteractionTime = Time.time;
        }
        else if (Input.GetMouseButton(0))
        {
            itemType.Interact(blockedMouseLocation, 0, false);
            lastBlockInteractionTime = Time.time;
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
        var topmostBlock = Chunk.GetTopmostBlock(x, Location.dimension, false);

        if (topmostBlock == null) return new Location(x, 80, Location.dimension);

        return topmostBlock.location + new Location(0, 2);
    }

    public override void Die()
    {
        DeathMenu.active = true;
        health = 20;
        hunger = 20;

        base.Die();
        transform.position = ValidSpawn(spawnLocation.x).GetPosition();
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

    private IEnumerator MoveToValidSpawnOnceLoaded()
    {
        yield return new WaitForSeconds(2f);
        while (!IsChunkLoaded()) yield return new WaitForSeconds(2f);

        highestYlevelsinceground = 0; //Reset falldamage
        transform.position = ValidSpawn(spawnLocation.x).GetPosition();
    }
}