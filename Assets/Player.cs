﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Player : HumanEntity
{
    //Entity Properties
    public override bool chunk_loading { get; } = true;
    public override float maxHealth { get; } = 20;
    public float maxHunger = 20;
    public float reach = 5;


    //Entity Data Tags
    [EntityDataTag(false)]
    public float hunger;
    [EntityDataTag(true)]
    public PlayerInventory inventory = new PlayerInventory();
    [EntityDataTag(true)]
    public Vector2 spawnPosition = new Vector2(0, 80);

    //Entity State
    [Space]
    public static Player localInstance;
    public GameObject crosshair;
    private float lastFrameScroll;
    private float lastHitTime;
    private bool hasTouchedGround = false;
    private bool inventoryOpenLastFrame = false;

    public override void Start()
    {
        localInstance = this;
        
        hunger = maxHunger;
        inventory = new PlayerInventory();

        base.Start();
    }

    public override void Update()
    {
        base.Update();

        float scroll = Input.mouseScrollDelta.y;
        //Check once every 5 frames
        if (scroll != 0 && (Time.frameCount % 5 == 0 || lastFrameScroll == 0))
        {
            inventory.selectedSlot += (scroll > 0) ? -1 : 1;

            if (inventory.selectedSlot > 8)
                inventory.selectedSlot = 0;
            if (inventory.selectedSlot < 0)
                inventory.selectedSlot = 8;
        }
        lastFrameScroll = scroll;


        performInput();
        inventoryOpenLastFrame = InventoryMenuManager.instance.anyInventoryOpen();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    private void performInput()
    {
        if (Input.GetKeyDown(KeyCode.E) && !inventoryOpenLastFrame)
            inventory.Open(Vector2Int.RoundToInt(transform.position));

        if (Inventory.anyOpen)
            return;

        //Movement
        if (Input.GetKey(KeyCode.A))
        {
            Walk(-1);
        }
        if (Input.GetKey(KeyCode.D))
        {
            Walk(1);
        }
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space))
        {
            Jump();
        }
        
        //Inventory Managment
        if (Input.GetKeyDown(KeyCode.Q))
            DropSelected();

        KeyCode[] numpadCodes = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5
                                , KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9};
        foreach(KeyCode keyCode in numpadCodes)
        {
            if (Input.GetKeyDown(keyCode))
                inventory.selectedSlot = System.Array.IndexOf<KeyCode>(numpadCodes, keyCode);
        }

        //Crosshair
        Crosshair();
    }

    private void Crosshair() {
        if (WorldManager.instance.loadingProgress != 1)
            return;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int blockedMousePosition = (Vector2Int)Vector3Int.RoundToInt(mousePosition);
        mousePosition.z = 0;
        Block block = Chunk.getBlock(blockedMousePosition);
        bool isInRange = (Mathf.Abs(((Vector3)mousePosition - transform.position).magnitude) <= reach);
        bool isAboveEntity = false;
            

        //Hit Entities
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
        if (hit.collider != null && hit.transform.GetComponent<Entity>() != null)
        {
            isAboveEntity = true;

            if (Input.GetMouseButtonDown(0) && Time.time > lastHitTime + 0.5f && isInRange)
            {
                hit.transform.GetComponent<Entity>().Hit(1);
                lastHitTime = Time.time;
            }
        }


        crosshair.transform.position = (Vector2)blockedMousePosition;
        crosshair.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/crosshair_" + (isInRange ? (isAboveEntity ? "entity" : "full") : "empty"));

        if (isInRange)
        {
            if (System.Type.GetType(inventory.getSelectedItem().material.ToString()).IsSubclassOf(typeof(Block)))
            {
                if (block == null || (block.GetMaterial() == Material.Water || block.GetMaterial() == Material.Lava))
                {
                    if (Input.GetMouseButtonDown(1))
                    {
                        if (inventory.getSelectedItem().material != Material.Air &&
                            (inventory.getSelectedItem().amount > 0))
                        {

                            Chunk.setBlock((Vector2Int)Vector3Int.RoundToInt(mousePosition), inventory.getSelectedItem().material);
                            inventory.setItem(inventory.selectedSlot,
        new ItemStack(inventory.getSelectedItem().material, inventory.getSelectedItem().amount - 1));
                        }
                    }
                }
                else
                {
                    Item sampleItem = (Item)System.Activator.CreateInstance(typeof(Item));

                    if (Input.GetMouseButtonDown(1))
                    {
                        sampleItem.Interact(blockedMousePosition, 1, true);
                    }
                    else if (Input.GetMouseButton(1))
                    {
                        sampleItem.Interact(blockedMousePosition, 1, false);
                    }

                    if (Input.GetMouseButtonDown(0))
                    {
                        sampleItem.Interact(blockedMousePosition, 0, true);
                    }
                    else if (Input.GetMouseButton(0))
                    {
                        sampleItem.Interact(blockedMousePosition, 0, false);
                    }
                }
            }
            else if (System.Type.GetType(inventory.getSelectedItem().material.ToString()).IsSubclassOf(typeof(Item)))
            {
                Item item = (Item)System.Activator.CreateInstance(System.Type.GetType(inventory.getSelectedItem().material.ToString()));

                if (Input.GetMouseButtonDown(1))
                {
                    item.Interact(blockedMousePosition, 1, true);
                }
                else if (Input.GetMouseButton(1))
                {
                    item.Interact(blockedMousePosition, 1, false);
                }

                if (Input.GetMouseButtonDown(0))
                {
                    item.Interact(blockedMousePosition, 0, true);
                }
                else if (Input.GetMouseButton(0))
                {
                    item.Interact(blockedMousePosition, 0, false);
                }
            }
        }
    }

    public override List<ItemStack> GetDrops()
    {
        List<ItemStack> result = new List<ItemStack>();

        result.AddRange(inventory.items);

        return result;
    }

    public void DropSelected()
    {
        ItemStack item = inventory.getSelectedItem().Clone();

        if (item.amount <= 0)
            return;

        item.amount = 1;
        inventory.getSelectedItem().amount --;

        item.Drop(Vector2Int.CeilToInt(transform.position + new Vector3(4, 0)));
    }

    public override void DropAllDrops()
    {
        base.DropAllDrops();

        inventory.Clear();
    }

    public static Vector2 ValidSpawn(int pos)
    {
        for (int i = Chunk.Height; i <= 0; i --)
        {
            if(Chunk.getBlock(new Vector2Int((int)pos, i)) != null && Chunk.getBlock(new Vector2Int((int)pos, i)).playerCollide)
            {
                return new Vector2(pos, i+2);
            }
        }
        return new Vector2(pos, 80);
    }

    public override void Die()
    {
        DeathMenu.active = true;
        health = 20;
        hunger = 20;

        base.Die();
        transform.position = ValidSpawn((int)spawnPosition.x);
        Save();
    }

    public override void Hit(float damage)
    {

    }

    //Disable fall damage when player is spawned
    public override void TakeFallDamage(float damage)
    {
        if (hasTouchedGround)
        {
            base.TakeFallDamage(damage);
        }
        else
        {
            hasTouchedGround = true;
            return;
        }
    }

    public void Sleep()
    {
        int currentDay = (int)(WorldManager.world.time/WorldManager.dayLength);
        float newTime = (currentDay+1) * WorldManager.dayLength;
        bool isNight = (WorldManager.world.time % WorldManager.dayLength) > (WorldManager.dayLength/2);

        if (isNight)
        {
            Debug.LogError("slept");
            WorldManager.world.time = newTime;
        }
        else
        {
            Debug.LogError("cant sleep at day");
        }
    }

    public override string SavePath()
    {
        return WorldManager.world.getPath() + "\\players\\player.dat";
    }

    private Dictionary<string, string> dataFromString(string[] lines)
    {
        Dictionary<string, string> resultData = new Dictionary<string, string>();

        foreach (string line in lines)
        {
            if (line.Contains("="))
                resultData.Add(line.Split('=')[0], line.Split('=')[1]);
        }

        return resultData;
    }
}
