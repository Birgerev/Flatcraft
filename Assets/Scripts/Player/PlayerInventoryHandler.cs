using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerInventoryHandler : NetworkBehaviour
{
    private Material _actionBarLastSelectedMaterial;
    private int _framesSinceInventoryOpen;
    
    private Player _player;
    
    private void Update()
    {
        if(isServer) GetInventory().holder = _player.Location;
        
        if (!isOwned) return;

        ActionBarMessageUpdate();

        if (Inventory.IsAnyOpen(_player.playerInstance))
            _framesSinceInventoryOpen = 0;
        else
            _framesSinceInventoryOpen++;
        
        PerformInput();
    }

    [Client]
    private void PerformInput()
    {
        if (!PlayerInteraction.CanInteractWithWorld()) return;
        
        //Open inventory
        if (Input.GetKeyDown(KeyCode.E) && _framesSinceInventoryOpen > 10) CMD_OpenInventory();

        if (Input.GetKeyDown(KeyCode.Q)) CMD_DropSelected();

        HotbarSlotInput();
    }

    private void HotbarSlotInput()
    {
        //Number Keybinds
        KeyCode[] numpadCodes = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9 };
        foreach (KeyCode keyCode in numpadCodes)
            if (Input.GetKeyDown(keyCode))
                CMD_UpdateSelectedSlot(Array.IndexOf(numpadCodes, keyCode));
        
        //Hotbar scroll
        float scrollAmount = -Input.mouseScrollDelta.y;
        if (scrollAmount != 0)
        {
            int newSelectedSlot = GetInventory().selectedSlot + (int)scrollAmount;
            newSelectedSlot = (newSelectedSlot + 9) % 9; //Make sure it isnt negative

            CMD_UpdateSelectedSlot(newSelectedSlot);
        }
    }

    [Client]
    private void ActionBarMessageUpdate()
    {
        Material selectedMaterial = GetInventory().GetSelectedItem().material;

        if (selectedMaterial != _actionBarLastSelectedMaterial && selectedMaterial != Material.Air)
            ActionBar.message = selectedMaterial.ToString().Replace('_', ' ');

        _actionBarLastSelectedMaterial = selectedMaterial;
    }
    
    [Command]
    public void CMD_DropSelected()
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
    private void CMD_OpenInventory()
    {
        GetInventory().Open(_player.playerInstance);
    }

    [Command]
    private void CMD_UpdateSelectedSlot(int slot)
    {
        GetInventory().selectedSlot = slot;
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
    
    private void Awake()
    {
        _player = GetComponent<Player>();
    }
}
