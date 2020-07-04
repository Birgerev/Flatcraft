using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Player : HumanEntity
{
    //Entity Properties
    public override bool ChunkLoadingEntity { get; } = true;
    public override float maxHealth { get; } = 20;
    public float maxHunger = 20;
    public float reach = 5;
    public static float blockHitsPerPerSecond = 4.5f;


    //Entity Data Tags
    [EntityDataTag(false)]
    public float hunger;
    [EntityDataTag(true)]
    public PlayerInventory inventory = new PlayerInventory();
    [EntityDataTag(true)]
    public Location spawnLocation = new Location(0, 80);

    //Entity State
    [Space]
    public static Player localInstance;
    public GameObject crosshair;
    private float lastFrameScroll;
    private float lastHitTime;
    private bool inventoryOpenLastFrame = false;
    private float lastBlockHit;

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

        performInput();
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

        inventoryOpenLastFrame = InventoryMenuManager.instance.anyInventoryOpen();
    }

    private void performInput()
    {
        if (Input.GetKeyDown(KeyCode.E) && !inventoryOpenLastFrame)
            inventory.Open(location);

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
        MouseInput();
    }

    private void MouseInput() {
        if (WorldManager.instance.loadingProgress != 1)
            return;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Location blockedMouseLocation = Location.locationByPosition(mousePosition, location.dimension);
        mousePosition.z = 0;
        Block crosshairBlock = Chunk.getBlock(blockedMouseLocation);
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


        crosshair.transform.position = blockedMouseLocation.getPosition();
        crosshair.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/crosshair_" + (isInRange ? (isAboveEntity ? "entity" : "full") : "empty"));

        
        
        if (!isInRange)
            return;

        if (Time.time - lastBlockHit < 1 / blockHitsPerPerSecond)
            return;
        
        
        Item itemType;
        if(System.Type.GetType(inventory.getSelectedItem().material.ToString()).IsSubclassOf(typeof(Item)))    //if the selected item derives from "Item", create in instance of item, else create basic "Item", without any subclasses
            itemType = (Item)System.Activator.CreateInstance(System.Type.GetType(inventory.getSelectedItem().material.ToString()));    
        else
            itemType = (Item)System.Activator.CreateInstance(typeof(Item));    

        if (System.Type.GetType(inventory.getSelectedItem().material.ToString()).IsSubclassOf(typeof(Block)) || System.Type.GetType(inventory.getSelectedItem().material.ToString()).IsSubclassOf(typeof(PlaceableItem)))
        {
            if (crosshairBlock == null || (crosshairBlock.GetMaterial() == Material.Water || crosshairBlock.GetMaterial() == Material.Lava))
            {
                if (Input.GetMouseButtonDown(1))
                {
                    if (inventory.getSelectedItem().material != Material.Air &&
                        (inventory.getSelectedItem().amount > 0))
                    {
                        if(System.Type.GetType(inventory.getSelectedItem().material.ToString()).IsSubclassOf(typeof(PlaceableItem)))
                            Chunk.setBlock(blockedMouseLocation, ((PlaceableItem)itemType).blockMaterial);
                        if (System.Type.GetType(inventory.getSelectedItem().material.ToString()).IsSubclassOf(typeof(Block)))
                            Chunk.setBlock(blockedMouseLocation, inventory.getSelectedItem().material);
                        
                        inventory.setItem(inventory.selectedSlot,
                            new ItemStack(inventory.getSelectedItem().material,
                                inventory.getSelectedItem().amount - 1));
                        
                        lastBlockHit = Time.time;
                        return;
                    }
                }
            }
        }


        if (Input.GetMouseButtonDown(1))
        {
            itemType.Interact(blockedMouseLocation, 1, true);
            lastBlockHit = Time.time;
        }
        else if (Input.GetMouseButton(1))
        {
            itemType.Interact(blockedMouseLocation, 1, false);
            lastBlockHit = Time.time;
        }

        if (Input.GetMouseButtonDown(0))
        {
            itemType.Interact(blockedMouseLocation, 0, true);
            lastBlockHit = Time.time;
        }
        else if (Input.GetMouseButton(0))
        {
            itemType.Interact(blockedMouseLocation, 0, false);
            lastBlockHit = Time.time;
        }
    }

    public void DoToolDurability()
    {
        if (inventory.getSelectedItem().getMaxDurability() != -1)
        {
            inventory.getSelectedItem().durablity--;

            if (inventory.getSelectedItem().durablity < 0)
                inventory.setItem(inventory.selectedSlot, new ItemStack());
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

        item.Drop(location + new Location(1 * (facingLeft ? -1 : 1), 0), new Vector2(3 * (facingLeft ? -1 : 1), 0));
    }

    public override void DropAllDrops()
    {
        base.DropAllDrops();

        inventory.Clear();
    }

    public Location ValidSpawn(int pos)
    {
        Dimension curDimension = location.dimension;

        for (int i = Chunk.Height; i > 0; i --)
        {
            if(Chunk.getBlock(new Location((int)pos, i, curDimension)) != null && Chunk.getBlock(new Location((int)pos, i, curDimension)).playerCollide)
            {
                return new Location(pos, i+2, curDimension);
            }
        }
        return new Location(pos, 80, curDimension);
    }

    public override void Die()
    {
        DeathMenu.active = true;
        health = 20;
        hunger = 20;

        base.Die();
        transform.position = ValidSpawn((int)spawnLocation.x).getPosition();
        UpdateCachedPosition();
        Save();
    }

    public override void Damage(float damage)
    {
        base.Damage(damage);
        
        Sound.Play(location, "entity/Player/hurt", SoundType.Entities, 0.85f, 1.15f);    //Play hurt sound
    }

    public override void Hit(float damage)
    {
        
    }

    public void Sleep()
    {
        int currentDay = (int)(WorldManager.world.time/WorldManager.dayLength);
        float newTime = (currentDay+1) * WorldManager.dayLength;
        bool isNight = (WorldManager.world.time % WorldManager.dayLength) > (WorldManager.dayLength/2);

        if (isNight)
        {
            WorldManager.world.time = newTime;
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

    IEnumerator MoveToValidSpawnOnceLoaded()
    {
        yield return new WaitForSeconds(2f);
        while (!isChunkLoaded())
        {
            yield return new WaitForSeconds(2f);
        }

        highestYlevelsinceground = 0;    //Reset falldamage
        transform.position = ValidSpawn(spawnLocation.x).getPosition();
    }
}
