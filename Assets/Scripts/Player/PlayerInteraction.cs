using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerInteraction : NetworkBehaviour
{
    public const float InteractionsPerPerSecond = 4.5f;
    
    [HideInInspector]public GameObject crosshair;
    
    [HideInInspector] [SyncVar] public double lastBlockHitTime;
    [HideInInspector] [SyncVar] public double lastHitTime;
    public double lastBlockInteractionTime;
    
    private Player _player;
    
    private void Update()
    {
        if (!isOwned) return;
        if (!_player.IsChunkLoaded()) return;
        if (Inventory.IsAnyOpen(_player.playerInstance.GetComponent<PlayerInstance>())) return;
        if (ChatMenu.instance.open) return;
        if (SignEditMenu.IsLocalMenuOpen()) return;

        UpdateCrosshair();
        EatItemInput();
        
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float mouseDistance = Vector2.Distance(mousePosition, transform.position);
        
        if (mouseDistance > _player.reach) return;

        InteractEntityInput();
        BlockPlaceInput();
        BlockInteractionInput();
    }

    [Client]
    private void InteractEntityInput()
    {
        Entity entity = GetMouseEntity();

        if (!entity) return;
        if (entity == _player) return;

        //Hit Entities
        if (Input.GetMouseButtonDown(0))
            CMD_HitEntity(entity);
        
        //Interract Entities
        if (Input.GetMouseButtonDown(1))
            CMD_InteractEntity(entity);
    }
    
    [Client]
    private void BlockPlaceInput()
    {
        if (!Input.GetMouseButtonDown(1)) return;
        if (GetMouseEntity()) return;
        
        Location loc = GetBlockedMouseLocation();
        Material currentMaterial = loc.GetMaterial();
        
        //TODO replace with CanOverride()
        if (currentMaterial != Material.Air && currentMaterial != Material.Water && currentMaterial != Material.Lava) return;
        
        CMD_PlaceBlock(loc);
    }

    [Command]
    public void CMD_PlaceBlock(Location loc)
    {
        ItemStack selectedItem = _player.GetInventory().GetSelectedItem();

        if (selectedItem.material == Material.Air) return;
        if (selectedItem.Amount <= 0) return;
        
        Material heldMat = selectedItem.material;
        
        if (Type.GetType(selectedItem.material.ToString()).IsSubclassOf(typeof(PlaceableItem)))
            heldMat = ((PlaceableItem) Activator.CreateInstance(Type.GetType(selectedItem.material.ToString()))).blockMaterial;
        
        if (Type.GetType(selectedItem.material.ToString()).IsSubclassOf(typeof(Item))) return;

        loc.SetMaterial(heldMat);
        loc.GetBlock().BuildTick();
        loc.Tick();

        _player.GetInventory().SetItem(_player.GetInventory().selectedSlot, new ItemStack(selectedItem.material, selectedItem.Amount - 1));
        
        //Trigger animation
        lastBlockInteractionTime = NetworkTime.time;
    }

    [Client]
    private void BlockInteractionInput()
    {
        if (NetworkTime.time - lastBlockInteractionTime < 1f / InteractionsPerPerSecond) return;
        
        if (Input.GetMouseButtonDown(1))
            CMD_Interact(GetBlockedMouseLocation(), 1, true);
        else if (Input.GetMouseButton(1))
            CMD_Interact(GetBlockedMouseLocation(), 1, false);
        
        if (Input.GetMouseButtonDown(0))
            CMD_Interact(GetBlockedMouseLocation(), 0, true);
        else if (Input.GetMouseButton(0))
        {
            CMD_Interact(GetBlockedMouseLocation(), 0, false);
            _player.ShakeClientCamera(10f * Time.deltaTime);
        }
        
        if(Input.GetMouseButton(0) || Input.GetMouseButton(1))
            lastBlockInteractionTime = NetworkTime.time;
    }

    [Command]
    public void CMD_Interact(Location loc, int mouseButton, bool firstFrameDown, NetworkConnectionToClient sender = null)
    {
        //if the selected item derives from "Item", create in instance of item, else create empty
        //"Item", without any subclasses
        Type itemType = Type.GetType(_player.GetInventory().GetSelectedItem().material.ToString());
        if (!itemType.IsSubclassOf(typeof(Item))) itemType = typeof(Item);
            
        Item item = (Item) Activator.CreateInstance(itemType);
        PlayerInstance player = sender.identity.GetComponent<PlayerInstance>();

        item.Interact(player, loc, mouseButton, firstFrameDown);
        lastBlockHitTime = NetworkTime.time;
    }

    [Server]
    public void DoToolDurability()
    {
        PlayerInventory inv = _player.GetInventory();
        ItemStack item = inv.GetSelectedItem();
        item.ApplyDurability();

        inv.SetItem(inv.selectedSlot, item);
    }

    [Command]
    public virtual void CMD_HitEntity(Entity entity)
    {
        float damage = _player.GetInventory().GetSelectedItem().GetItemEntityDamage();
        
        //Critical hits
        if (_player.GetVelocity().y < -0.5f)
        {
            damage *= 1.5f;
            entity.CriticalDamageEffect();
        }

        Sound.Play(_player.Location, "entity/player/swing", SoundType.Entities, 0.8f, 1.2f);
        _player.ShakeOwnersCamera(.5f);
        DoToolDurability();
        
        entity.transform.GetComponent<Entity>().Hit(damage, _player);
        lastHitTime = NetworkTime.time;
        
    }
    
    [Command]
    public virtual void CMD_InteractEntity(Entity entity)
    {
        entity.Interact(_player);
        lastHitTime = NetworkTime.time;
    }

    [Client]
    private void UpdateCrosshair()
    {
        if (crosshair == null)
            crosshair = Instantiate(Resources.Load<GameObject>("Prefabs/Crosshair"));

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        bool isInRange = Vector2.Distance(mousePosition, transform.position) <= _player.reach;
        Entity mouseEntity = GetMouseEntity();

        string spriteName = "empty";
        if (isInRange)
            spriteName = mouseEntity == null ? "full" : "entity";

        crosshair.transform.position = GetBlockedMouseLocation().GetPosition();
        crosshair.GetComponent<SpriteRenderer>().sprite =
            Resources.Load<Sprite>("Sprites/crosshair_" + spriteName);
    }

    [Client]
    private void EatItemInput()
    {
        if (Input.GetMouseButton(1)) return;
        if (_player.hunger > _player.maxHunger - .5f) return;
        if (Time.time % 0.2f - Time.deltaTime <= 0) return;//TODO
        if (!Type.GetType(_player.GetInventory().GetSelectedItem().material.ToString()).IsSubclassOf(typeof(Food))) return;
        
        CMD_Eat();
    }
    
    [Command]
    public void CMD_Eat()
    {
        if (!Type.GetType(_player.GetInventory().GetSelectedItem().material.ToString()).IsSubclassOf(typeof(Food))) return;

        Sound.Play(_player.Location, "entity/Player/eat", SoundType.Entities, 0.85f, 1.15f);
        _player.eatingTime += 0.2f;

        if (_player.eatingTime < 1.3f) return;
        
        ConsumeHeldItem();
        _player.eatingTime = 0;
    }

    [Server]
    private void ConsumeHeldItem()
    {
        ItemStack selectedItemStack = _player.GetInventory().GetSelectedItem();
        Food foodItem = (Food) Activator.CreateInstance(Type.GetType(_player.GetInventory().GetSelectedItem().material.ToString()));
        
        _player.hunger = Mathf.Clamp(_player.hunger + foodItem.food_points, 0, _player.maxHunger);
        RPC_PlayEatEffect(selectedItemStack.GetTextureColors());
        Sound.Play(_player.Location, "entity/Player/burp", SoundType.Entities, 0.85f, 1.15f);

        //Subtract food item from inventory
        selectedItemStack.Amount--;
        _player.GetInventory().SetItem(_player.GetInventory().selectedSlot, selectedItemStack);
    }

    [ClientRpc]
    private void RPC_PlayEatEffect(Color[] colors)
    {
        System.Random r = new System.Random();
        for (int i = 0; i < r.Next(6, 10); i++) //SpawnParticles
        {
            Particle part = Particle.ClientSpawn();
            Color color = colors[r.Next(0, colors.Length)];
            part.transform.position = _player.Location.GetPosition() + new Vector2(0, 0.2f);
            part.color = color;
            part.doGravity = true;
            part.velocity = new Vector2((0.5f + (float) r.NextDouble()) * (r.Next(0, 2) == 0 ? -1 : 1),
                3f + (float) r.NextDouble());
            part.maxAge = (float) r.NextDouble();
            part.maxBounces = 10;
        }
    }
    
    [Client]
    public static Location GetBlockedMouseLocation()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Location blockedMouseLocation = Location.LocationByPosition(mousePosition);

        return blockedMouseLocation;
    }
    
    [Client]
    public static Entity GetMouseEntity()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        foreach (RaycastHit2D ray in Physics2D.RaycastAll(mousePosition, Vector2.zero))
        {
            Entity entity = ray.transform.GetComponent<Entity>();
            if (entity) return entity;
        }

        return null;
    }
    
    private void Awake()
    {
        _player = GetComponent<Player>();
    }

    private void OnDestroy()
    {
        Destroy(crosshair);
    }
}
