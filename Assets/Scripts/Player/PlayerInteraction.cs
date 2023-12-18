using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerInteraction : NetworkBehaviour
{
    public const float InteractionsPerPerSecond = 4.5f;
    
    public GameObject crosshair;
    
    private Player _player;
    //TODO tons of GetInventory() calls
    private void Update()
    {
        if (!isOwned) return;
        if (!_player.IsChunkLoaded()) return;
        if (Inventory.IsAnyOpen(_player.playerInstance.GetComponent<PlayerInstance>())) return;
        if (ChatMenu.instance.open) return;
        if (SignEditMenu.IsLocalMenuOpen()) return;

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        bool isInRange = Vector2.Distance(mousePosition, transform.position) <= _player.reach;

        UpdateCrosshair();
        EatItemInput();

        if (!isInRange) return;

        InteractEntityInput();
        BlockInteractionInput();
        BlockPlaceInput();
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
        ItemStack item = _player.GetInventory().GetSelectedItem();
        Material heldMat;

        if (_player.GetInventory().GetSelectedItem().material == Material.Air || _player.GetInventory().GetSelectedItem().Amount <= 0)
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

        _player.GetInventory().SetItem(_player.GetInventory().selectedSlot, new ItemStack(item.material, item.Amount - 1));
        _player.lastHitTime = NetworkTime.time;
    }

    [Client]
    private void BlockInteractionInput()
    {
        if (Time.time - _player.lastBlockInteractionTime >= 1f / InteractionsPerPerSecond)
        {
            if (Input.GetMouseButtonDown(1))
            {
                RequestInteract(GetBlockedMouseLocation(), 1, true);
                _player.lastBlockInteractionTime = Time.time;
            }
            else if (Input.GetMouseButton(1))
            {
                RequestInteract(GetBlockedMouseLocation(), 1, false);
                _player.lastBlockInteractionTime = Time.time;
            }

            if (Input.GetMouseButtonDown(0))
            {
                RequestInteract(GetBlockedMouseLocation(), 0, true);
                _player.lastBlockInteractionTime = Time.time;
            }
            else if (Input.GetMouseButton(0))
            {
                RequestInteract(GetBlockedMouseLocation(), 0, false);
                _player.lastBlockInteractionTime = Time.time;
                _player.ShakeClientCamera(10f * Time.deltaTime);
            }
        }
    }

    [Command]
    public void RequestInteract(Location loc, int mouseButton, bool firstFrameDown, NetworkConnectionToClient sender = null)
    {
        //if the selected item derives from "Item", create in instance of item, else create empty
        //"Item", without any subclasses
        Item itemType;
        if (Type.GetType(_player.GetInventory().GetSelectedItem().material.ToString()).IsSubclassOf(typeof(Item)))
            itemType = (Item) Activator.CreateInstance(
                Type.GetType(_player.GetInventory().GetSelectedItem().material.ToString()));
        else
            itemType = (Item) Activator.CreateInstance(typeof(Item));

        PlayerInstance player = sender.identity.GetComponent<PlayerInstance>();

        itemType.Interact(player, loc, mouseButton, firstFrameDown);
        if (loc.GetMaterial() != Material.Air)
            _player.lastBlockHitTime = NetworkTime.time;
    }

    [Server]
    public void DoToolDurability()
    {
        PlayerInventory inv = _player.GetInventory();
        
        if (inv.GetSelectedItem().GetMaxDurability() != -1)
        {
            ItemStack newItem = inv.GetSelectedItem();
            newItem.durability--;

            inv.SetItem(inv.selectedSlot, (newItem.durability >= 0) ? newItem : new ItemStack());
        }
    }

    [Command]
    public virtual void RequestHitEntity(GameObject entityObj)
    {
        Entity entity = entityObj.GetComponent<Entity>();
        PlayerInventory inv = _player.GetInventory();
        
        bool criticalHit = false;
        float damage = _player.GetInventory().GetSelectedItem().GetItemEntityDamage();
        
        if (_player.GetVelocity().y < -0.5f)
            criticalHit = true;

        if (criticalHit)
        {
            damage *= 1.5f;
            entity.CriticalDamageEffect();
        }

        DoToolDurability();

        entity.transform.GetComponent<Entity>().Hit(damage, _player);
        _player.lastHitTime = NetworkTime.time;
        
        Sound.Play(_player.Location, "entity/player/swing", SoundType.Entities, 0.8f, 1.2f);
        _player.ShakeOwnersCamera(.5f);
    }
    
    [Command]
    public virtual void RequestInteractEntity(GameObject entityObj)
    {
        Entity entity = entityObj.GetComponent<Entity>();

        entity.transform.GetComponent<Entity>().Interact(_player);
        _player.lastHitTime = NetworkTime.time;
    }

    
    [Client]
    private void UpdateCrosshair()
    {
        if (crosshair == null)
            crosshair = Instantiate(Resources.Load<GameObject>("Prefabs/Crosshair"));

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        bool isInRange = Vector2.Distance(mousePosition, transform.position) <= _player.reach;
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
    private void EatItemInput()
    {
        if (Input.GetMouseButtonUp(1))
        {
            RequestResetEatTime();
            return;
        }

        if (Input.GetMouseButton(1) && _player.hunger <= _player.maxHunger - .5f && Time.time % 0.2f - Time.deltaTime <= 0 &&
            Type.GetType(_player.GetInventory().GetSelectedItem().material.ToString()).IsSubclassOf(typeof(Food)))
            RequestEat();
    }
    
    
    [Command]
    public void RequestEat()
    {
        if (!Type.GetType(_player.GetInventory().GetSelectedItem().material.ToString()).IsSubclassOf(typeof(Food)))
            return;

        if (_player.eatingTime > 1.3f)
        {
            EatHeldItem();
            _player.eatingTime = 0;
            return;
        }

        Sound.Play(_player.Location, "entity/Player/eat", SoundType.Entities, 0.85f, 1.15f);

        _player.eatingTime += 0.2f;
    }

    [Command]
    public void RequestResetEatTime()
    {
        _player.eatingTime = 0;
    }

    [Server]
    private void EatHeldItem()
    {
        ItemStack selectedItemStack = _player.GetInventory().GetSelectedItem();

        //Add hunger
        Food foodItemType =
            (Food) Activator.CreateInstance(Type.GetType(_player.GetInventory().GetSelectedItem().material.ToString()));
        int foodPoints = foodItemType.food_points;
        _player.hunger = Mathf.Clamp(_player.hunger + foodPoints, 0, _player.maxHunger);

        //Particle Effect
        PlayEatEffect(selectedItemStack.GetTextureColors());

        //Burp sounds
        Sound.Play(_player.Location, "entity/Player/burp", SoundType.Entities, 0.85f, 1.15f);

        //Subtract food item from inventory
        selectedItemStack.Amount -= 1;
        _player.GetInventory().SetItem(_player.GetInventory().selectedSlot, selectedItemStack);
    }

    [ClientRpc]
    private void PlayEatEffect(Color[] colors)
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
    
    private void Awake()
    {
        _player = GetComponent<Player>();
    }

    private void OnDestroy()
    {
        Destroy(crosshair);
    }
}
