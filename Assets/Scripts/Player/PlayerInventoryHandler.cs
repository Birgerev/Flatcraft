using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerInventoryHandler : NetworkBehaviour
{
    private Material _actionBarLastSelectedMaterial;
    private int _framesSinceInventoryOpen;
    private float _lastFrameScroll;
    
    private Player _player;
    
    private void Update()
    {
        if(isServer)
            GetInventory().holder = _player.Location;
        
        if (!isOwned) return;

        CheckActionBarUpdate();

        if (Inventory.IsAnyOpen(_player.playerInstance))
            _framesSinceInventoryOpen = 0;
        else
            _framesSinceInventoryOpen++;
        
        PerformInput();
    }

    [Client]
    private void PerformInput()
    {
        //Open inventory
        if (Input.GetKeyDown(KeyCode.E) && _framesSinceInventoryOpen > 10)
            RequestOpenInventory();

        //Inventory Managment
        if (Input.GetKeyDown(KeyCode.Q))
            RequestDropItem();

        KeyCode[] numpadCodes =
        {
            KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6,
            KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9
        };
        foreach (KeyCode keyCode in numpadCodes)
            if (Input.GetKeyDown(keyCode))
                SetSelectedInventorySlot(Array.IndexOf(numpadCodes, keyCode));


        float scroll = Input.mouseScrollDelta.y;
        //Check once every 5 frames
        if (scroll != 0 && (Time.frameCount % 5 == 0 || _lastFrameScroll == 0))
        {
            int newSelectedSlot = GetInventory().selectedSlot + (scroll > 0 ? -1 : 1);
            if (newSelectedSlot > 8)
                newSelectedSlot = 0;
            if (newSelectedSlot < 0)
                newSelectedSlot = 8;

            SetSelectedInventorySlot(newSelectedSlot);
        }

        _lastFrameScroll = scroll;
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
        
        droppedItem.Drop(_player.Location + new Location(1 * (_player.facingLeft ? -1 : 1), 1), 
            new Vector2(3 * (_player.facingLeft ? -1 : 1), 0f));
    }
    
    [Command]
    private void RequestOpenInventory()
    {
        GetInventory().Open(_player.playerInstance);
    }

    [Command]
    private void RequestDropItem()
    {
        DropSelected();
    }

    public PlayerInventory GetInventory()
    {
        Inventory inventory = Inventory.Get(_player.inventoryId);
        
        //Create an inventory if it doesn't exist
        if (inventory == null)
        {
            inventory = PlayerInventory.CreatePreset();
            _player.inventoryId = inventory.id;
        }
        
        return (PlayerInventory)inventory;
    }

    [Command]
    private void SetSelectedInventorySlot(int slot)
    {
        GetInventory().selectedSlot = slot;
    }

    [Client]
    private void CheckActionBarUpdate()
    {
        Material selectedMaterial = GetInventory().GetSelectedItem().material;

        if (selectedMaterial != _actionBarLastSelectedMaterial && selectedMaterial != Material.Air)
            ActionBar.message = selectedMaterial.ToString().Replace('_', ' ');

        _actionBarLastSelectedMaterial = selectedMaterial;
    }

    private void Awake()
    {
        _player = GetComponent<Player>();
    }
}
