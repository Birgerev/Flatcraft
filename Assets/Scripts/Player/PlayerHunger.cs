
using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerHunger : NetworkBehaviour
{
    private static readonly float HealthRegenerationHungerCost = 0.4f;
    private static readonly float JumpHungerCost = 0.1f;
    private static readonly float MovementHungerCost = 0.03f;
    private static readonly float SprintHungerCost = 0.03f;
    
    public float maxHunger = 20;
    
    [HideInInspector] public float eatingTime;
    
    private Player _player;
    //TODO clean entire script
    private void Update()
    {
        if (isOwned) EatItemInput();
        
        if (!isServer) return;
        
        CheckRegenerateHealth();
        
        if (Time.time % 1f < Time.deltaTime)
        {
            if (_player.GetVelocity().x > 0.2f || _player.GetVelocity().x < -0.2f)
                _player.hunger -= MovementHungerCost;
            if (_player.sprinting)
                _player.hunger -= SprintHungerCost;
            if (_player.GetVelocity().y > 0)
                _player.hunger -= JumpHungerCost;
        }
    }
    
    [Server]
    private void CheckRegenerateHealth()
    {
        if ( _player.health >= 20)
            return;

        if ( _player.hunger > 19)
        {
            if (Time.time % 0.5f <= Time.deltaTime)
            {
                _player.health += 1;
                _player.hunger -= HealthRegenerationHungerCost;
            }
        }
        else if ( _player.hunger > 17)
        {
            if (Time.time % 4f <= Time.deltaTime)
            {
                _player.health += 1;
                _player.hunger -= HealthRegenerationHungerCost;
            }
        }
    }
    
    [Client]
    private void EatItemInput()
    {
        if (Input.GetMouseButtonUp(1)) CMD_StopEat();
        
        if (!Input.GetMouseButton(1)) return;
        if (_player.hunger > maxHunger - .5f) return;
        if (Time.time % 0.2f > Time.deltaTime) return;
        if (!Type.GetType(_player.GetInventory().GetSelectedItem().material.ToString()).IsSubclassOf(typeof(Food))) return;
        
        CMD_Eat();
    }

    [Command]
    private void CMD_Eat()
    {
        if (!Type.GetType(_player.GetInventory().GetSelectedItem().material.ToString()).IsSubclassOf(typeof(Food))) return;

        Sound.Play(_player.Location, "entity/Player/eat", SoundType.Entities, 0.85f, 1.15f);
        eatingTime += 0.2f;

        if (eatingTime < 1.3f) return;
        
        ConsumeHeldItem();
        eatingTime = 0;
    }

    [Command]
    private void CMD_StopEat()
    {
        eatingTime = 0;
    }

    [Server]
    private void ConsumeHeldItem()
    {
        ItemStack selectedItemStack = _player.GetInventory().GetSelectedItem();
        Food foodItem = (Food) Activator.CreateInstance(Type.GetType(_player.GetInventory().GetSelectedItem().material.ToString()));
        
        _player.hunger = Mathf.Clamp(_player.hunger + foodItem.food_points, 0, maxHunger);
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

    private void Awake()
    {
        _player = GetComponent<Player>();
    }
}
