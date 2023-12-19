using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Mirror;
using UnityEngine;
using Random = System.Random;

public class Player : LivingEntity
{
    //Entity State
    public static Player LocalEntity;
    public static List<Player> Players = new ();

    [SyncVar] public PlayerInstance playerInstance;

    //Entity Data Tags
    [EntityDataTag(false)] [SyncVar] public float hunger;

    [EntityDataTag(false)] [SyncVar] public int inventoryId = 0;

    public Location bedLocation = new Location(0, 0);


    private Material actionBarLastSelectedMaterial;
    private int framesSinceInventoryOpen;


    //Entity Properties
    public override bool ChunkLoadingEntity { get; } = true;
    public override float maxHealth { get; } = 20;

    public override void Start()
    {
        Debug.Log("Spawning player '" + uuid + "'");
        Players.Add(this);

        base.Start();
    }

    public void OnDestroy()
    {
        Players.Remove(this);
    }

    [Client]
    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        LocalEntity = this;
        CameraController.instance.target = transform;
    }

    [Server]
    public override void Spawn()
    {
        base.Spawn();

        hunger = GetComponent<PlayerHunger>().maxHunger;

        if (bedLocation.GetMaterial() == Material.Bed_Bottom || bedLocation.GetMaterial() == Material.Bed_Top)
            Teleport(bedLocation);

        StartCoroutine(ValidSpawnOnceChunkLoaded());
    }

    [Server]
    public override void Tick()
    {
        base.Tick();
        
        CalculateFlip();
        GetInventory().holder = Location;
        CheckStarvationDamage();
        ClimbableSound();
    }

    [Client]
    public override void ClientUpdate()
    {
        //Crosshair
        if (isOwned)
        {
            CheckActionBarUpdate();
            CheckDimensionChangeLoadingScreen();

            if (Inventory.IsAnyOpen(playerInstance))
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
        if (isOwned)
            Location = loc;
    }

    [Client]
    public override void UpdateNameplate()
    {
        if (isOwned)
        {
            nameplate.text = "";
            return;
        }

        base.UpdateNameplate();
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
        
        droppedItem.Drop(Location + new Location(1 * (facingLeft ? -1 : 1), 1)
            , new Vector2(3 * (facingLeft ? -1 : 1), 0f));
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
        
        ChatManager.instance.AddMessage(displayName + " died");
        File.Delete(SavePath());
        GetInventory().Delete();
        DeathMenuEffect();
    }

    [ClientRpc]
    public override void Knockback(Vector2 direction)
    {
        if (isOwned)
            ClientKnockback(direction);
    }

    public void ClientKnockback(Vector2 direction)
    {
        base.Knockback(direction);
    }

    [ClientRpc]
    public void DeathMenuEffect()
    {
        if (isOwned)
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

        ShakeOwnerCamera(5);
    }

    [ClientRpc]
    public void ShakeOwnerCamera(float shakeValue)
    {
        if (isOwned)
            CameraController.ShakeClientCamera(shakeValue);
    }

    [Server]
    private void CalculateFlip()
    {
        if (Mathf.Abs(GetVelocity().x) > 0.1f)
            facingLeft = GetVelocity().x < 0;
    }
    
    [Server]
    public override void UpdateAnimatorValues()
    {
        base.UpdateAnimatorValues();

        Animator anim = GetComponent<Animator>();
        
        anim.SetBool("eating", GetComponent<PlayerHunger>().eatingTime > 0);
        anim.SetBool("punch", 
            NetworkTime.time - GetComponent<PlayerInteraction>().lastHitTime < 0.05f || 
                NetworkTime.time - GetComponent<PlayerInteraction>().lastBlockInteractionTime < 0.1f || 
                NetworkTime.time - GetComponent<PlayerInteraction>().lastBlockHitTime < 0.3f);
        anim.SetBool("holding-item", GetInventory().GetSelectedItem().material != Material.Air);
        anim.SetBool("sneaking", GetComponent<PlayerMovement>().sneaking);
    }
    
    
    //TODO move
    private float lastFrameScroll;//TODO _ name
    
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

    public PlayerInventory GetInventory()
    {
        Inventory inventory = Inventory.Get(inventoryId);
        
        //Create an inventory if it doesn't exist
        if (inventory == null)
        {
            inventory = PlayerInventory.CreatePreset();
            inventoryId = inventory.id;
        }
        
        return (PlayerInventory)inventory;
    }

    [Command]
    private void SetSelectedInventorySlot(int slot)
    {
        GetInventory().selectedSlot = slot;
    }

}