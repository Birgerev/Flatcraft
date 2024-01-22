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
    public const float Reach = 5;
    
    [HideInInspector] public GameObject crosshair;
    [HideInInspector] public double lastBlockHitTime;
    [HideInInspector] public double lastHitTime;
    [HideInInspector] public double lastBlockInteractionTime;
    
    private Player _player;
    
    private void Update()
    {
        if (!isOwned) return;
        if (!CanInteractWithWorld()) return;

        UpdateCrosshair();
        
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float mouseDistance = Vector2.Distance(mousePosition, transform.position);
        
        if (mouseDistance > Reach) return;

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
        
        CMD_TryPlaceBlock(loc);
    }

    [Command]
    public void CMD_TryPlaceBlock(Location loc)
    {
        ItemStack selectedItem = _player.GetInventoryHandler().GetInventory().GetSelectedItem();

        if (selectedItem.material == Material.Air) return;
        if (selectedItem.Amount < 1) return;
        
        Material materialToPlace = selectedItem.material;
        Type materialType = Type.GetType(selectedItem.material.ToString());
        
        if (materialType.IsSubclassOf(typeof(Item)))
            if (materialType.IsSubclassOf(typeof(PlaceableItem)))
                materialToPlace = ((PlaceableItem) Activator.CreateInstance(materialType)).blockMaterial;
            else
                return;
        
        //Check if block can be placed
        Block blockClass = (Block) Activator.CreateInstance(Type.GetType(materialToPlace.ToString()));
        if (!blockClass.CanExistAt(loc)) return;
        
        //Can current material be overriden?
        Block currentBlock = loc.GetBlock();
        if (currentBlock != null && !currentBlock.CanBeOverriden) return;
        
        loc.SetMaterial(materialToPlace);
        loc.GetBlock().BuildTick();
        loc.Tick();

        _player.GetInventoryHandler().GetInventory().ConsumeSelectedItem();
        
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
            CameraController.ShakeClientCamera(10f * Time.deltaTime);
        }
        
        if(Input.GetMouseButton(0) || Input.GetMouseButton(1))
            lastBlockInteractionTime = NetworkTime.time;
    }

    [Command]
    public void CMD_Interact(Location loc, int mouseButton, bool firstFrameDown, NetworkConnectionToClient sender = null)
    {
        //if the selected item derives from "Item", create in instance of item, else create empty
        //"Item", without any subclasses
        Type itemType = Type.GetType(_player.GetInventoryHandler().GetInventory().GetSelectedItem().material.ToString());
        if (!itemType.IsSubclassOf(typeof(Item))) itemType = typeof(Item);
            
        Item item = (Item) Activator.CreateInstance(itemType);
        PlayerInstance player = sender.identity.GetComponent<PlayerInstance>();

        item.Interact(player, loc, mouseButton, firstFrameDown);
        lastBlockHitTime = NetworkTime.time;
    }

    [Server]
    public void DoToolDurability()
    {
        PlayerInventory inv = _player.GetInventoryHandler().GetInventory();
        ItemStack item = inv.GetSelectedItem();
        
        item.ApplyDurability();
        inv.SetItem(inv.selectedSlot, item);
    }

    [Command]
    public virtual void CMD_HitEntity(Entity entity)
    {
        float damage = _player.GetInventoryHandler().GetInventory().GetSelectedItem().GetItemEntityDamage();
        
        //Critical hits
        if (_player.GetVelocity().y < -0.5f)
        {
            damage *= 1.5f;
            entity.GetComponent<EntityParticleEffects>()?.RPC_CriticalDamageEffect();
        }

        Sound.Play(_player.Location, "entity/player/swing", SoundType.Entities, 0.8f, 1.2f);
        _player.ShakeOwnerCamera(.5f);
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
        bool isInRange = Vector2.Distance(mousePosition, transform.position) <= Reach;
        Entity mouseEntity = GetMouseEntity();

        string spriteName = "empty";
        if (isInRange)
            spriteName = mouseEntity == null ? "full" : "entity";

        crosshair.transform.position = GetBlockedMouseLocation().GetPosition();
        crosshair.GetComponent<SpriteRenderer>().sprite =
            Resources.Load<Sprite>("Sprites/crosshair_" + spriteName);
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

    public static bool CanInteractWithWorld()
    {
        if (Inventory.IsAnyOpen(PlayerInstance.localPlayerInstance)) return false;
        if (ChatMenu.instance.open) return false;
        if (SignEditMenu.IsLocalMenuOpen()) return false;
        if (PauseMenu.active) return false;

        return true;
    }
}
